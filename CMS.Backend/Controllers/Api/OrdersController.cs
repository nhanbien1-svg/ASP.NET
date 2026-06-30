using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // MỚI: Cần thiết để dùng Include, AsNoTracking, ToListAsync
using CMS.Data;
using CMS.Data.Entities;
using CMS.Backend.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public OrdersController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // ==========================================
        // 1. API: NHẬN THÔNG TIN ĐẶT HÀNG TỪ REACTJS
        // ==========================================
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutDto request)
        {
            if (request == null || request.CartItems == null || !request.CartItems.Any())
            {
                return BadRequest(new { message = "Giỏ hàng trống hoặc dữ liệu không hợp lệ!" });
            }

            // TỐI ƯU: Sử dụng Transaction để đảm bảo tính toàn vẹn dữ liệu
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Tạo Đơn hàng chính (Order)
                var newOrder = new Order
                {
                    CustomerId = request.CustomerId,
                    OrderDate = DateTime.Now,
                    ShippingName = request.ShippingName?.Trim(),
                    ShippingPhone = request.ShippingPhone?.Trim(),
                    ShippingAddress = request.ShippingAddress?.Trim(),
                    Notes = request.Notes?.Trim(),
                    Status = 0, // 0: Chờ duyệt
                    PaymentMethod = string.IsNullOrEmpty(request.PaymentMethod) ? "COD" : request.PaymentMethod,
                    TotalAmount = request.TotalAmount
                };

                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync(); // Lưu để lấy được mã đơn hàng (Id)

                // 2. Kiểm tra tồn kho và Trừ số lượng (StockQuantity)
                var orderDetails = new List<OrderDetail>();

                foreach (var item in request.CartItems)
                {
                    // Lấy sản phẩm từ DB
                    var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);
                    
                    if (product == null)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { message = $"Sản phẩm có ID {item.ProductId} không tồn tại!" });
                    }

                    if (product.StockQuantity < item.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { message = $"Sản phẩm '{product.Name}' không đủ số lượng trong kho. Còn lại: {product.StockQuantity}." });
                    }

                    // Trừ tồn kho
                    product.StockQuantity -= item.Quantity;

                    // Thêm vào danh sách chi tiết đơn hàng
                    orderDetails.Add(new OrderDetail
                    {
                        OrderId = newOrder.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    });
                }

                _context.OrderDetails.AddRange(orderDetails);
                await _context.SaveChangesAsync();

                // 3. Nếu mọi thứ suôn sẻ, XÁC NHẬN lưu toàn bộ xuống DB
                await transaction.CommitAsync();

                // 4. GỬI EMAIL THÔNG BÁO TỚI KHÁCH HÀNG (Nếu khách hàng có email)
                var customer = await _context.Customers.FindAsync(request.CustomerId);
                if (customer != null && !string.IsNullOrEmpty(customer.Email))
                {
                    string subject = $"TechZone - Đặt hàng thành công! Mã đơn hàng: #{newOrder.Id}";
                    string htmlMessage = $@"
                        <h3>Cảm ơn bạn đã đặt hàng tại TechZone!</h3>
                        <p>Xin chào <strong>{customer.FullName}</strong>,</p>
                        <p>Đơn hàng <strong>#{newOrder.Id}</strong> của bạn đã được đặt thành công. Chúng tôi sẽ sớm liên hệ để giao hàng.</p>
                        <p><strong>Tổng tiền:</strong> {newOrder.TotalAmount:N0} đ</p>
                        <br/>
                        <p>Trân trọng,<br/>Đội ngũ TechZone</p>
                    ";
                    // Gửi email bất đồng bộ, bỏ qua lỗi nếu cấu hình sai để không làm gián đoạn việc đặt hàng
                    _ = _emailService.SendEmailAsync(customer.Email, subject, htmlMessage).ContinueWith(t => {
                        if (t.IsFaulted) {
                            Console.WriteLine("Lỗi gửi email: " + t.Exception?.Message);
                        }
                    });
                }

                return StatusCode(201, new { message = "Đặt hàng thành công!", orderId = newOrder.Id });
            }
            catch (Exception ex)
            {
                // Nếu có lỗi ở bất kỳ bước nào, HỦY BỎ toàn bộ thay đổi (Rollback)
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Lỗi hệ thống khi đặt hàng: " + ex.Message });
            }
        }

        // ==========================================
        // 2. API: LẤY LỊCH SỬ MUA HÀNG CỦA KHÁCH (MỚI BỔ SUNG)
        // ==========================================
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetCustomerOrders(int customerId)
        {
            var orders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.OrderDetails!)
                    .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new {
                    o.Id,
                    o.OrderDate,
                    o.TotalAmount,
                    o.Status,
                    o.PaymentMethod,
                    // Bóc tách danh sách sản phẩm để React dễ hiển thị
                    Products = o.OrderDetails!.Select(od => new {
                        Name = od.Product != null ? od.Product.Name : "Sản phẩm đã bị xóa",
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice
                    })
                })
                .ToListAsync();

            return Ok(orders);
        }

        // ==========================================
        // 3. API: HỦY ĐƠN HÀNG VÀ HOÀN TRẢ KHO
        // ==========================================
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Lấy đơn hàng cùng chi tiết đơn hàng
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return NotFound(new { message = "Không tìm thấy đơn hàng." });
                }

                // Chỉ cho phép hủy khi đang Chờ duyệt (Status = 0) hoặc một số trạng thái hợp lệ
                if (order.Status == 3) // 3 là Đã Hủy
                {
                    return BadRequest(new { message = "Đơn hàng này đã bị hủy từ trước." });
                }

                // Chuyển trạng thái thành Hủy
                order.Status = 3;

                // Hoàn trả lại số lượng vào kho
                foreach (var item in order.OrderDetails)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity; // Cộng lại kho
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // GỬI EMAIL THÔNG BÁO HỦY
                var customer = await _context.Customers.FindAsync(order.CustomerId);
                if (customer != null && !string.IsNullOrEmpty(customer.Email))
                {
                    string subject = $"TechZone - Đơn hàng #{order.Id} đã bị hủy";
                    string htmlMessage = $@"
                        <h3>Thông báo Hủy Đơn Hàng</h3>
                        <p>Xin chào <strong>{customer.FullName}</strong>,</p>
                        <p>Đơn hàng <strong>#{order.Id}</strong> của bạn đã bị hủy (Có thể do lỗi vận chuyển hoặc yêu cầu hủy).</p>
                        <p>Chúng tôi vô cùng xin lỗi vì sự bất tiện này.</p>
                        <br/>
                        <p>Trân trọng,<br/>Đội ngũ TechZone</p>
                    ";
                    _ = _emailService.SendEmailAsync(customer.Email, subject, htmlMessage).ContinueWith(t => {
                        if (t.IsFaulted) {
                            Console.WriteLine("Lỗi gửi email: " + t.Exception?.Message);
                        }
                    });
                }

                return Ok(new { message = "Hủy đơn hàng thành công! Số lượng sản phẩm đã được trả lại kho." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Lỗi khi hủy đơn: " + ex.Message });
            }
        }
    }

    // --- CÁC CLASS TRUNG GIAN ĐỂ HỨNG DỮ LIỆU TỪ REACT GỬI LÊN (DTO) ---
    public class CheckoutDto
    {
        public int CustomerId { get; set; }
        public string ShippingName { get; set; }
        public string ShippingPhone { get; set; }
        public string ShippingAddress { get; set; }
        public string? Notes { get; set; }
        public string PaymentMethod { get; set; } = "COD";
        public decimal TotalAmount { get; set; }
        public List<CartItemDto> CartItems { get; set; } = new List<CartItemDto>();
    }

    public class CartItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}