using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using CMS.Data;
using CMS.Data.Entities;
using System.Threading.Tasks;

namespace CMS.Backend.Controllers
{
    [Authorize] // Bảo mật: Bắt buộc Admin phải đăng nhập mới được xem
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CMS.Backend.Services.IEmailService _emailService;

        public OrderController(ApplicationDbContext context, CMS.Backend.Services.IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // ==========================================
        // 1. DANH SÁCH ĐƠN HÀNG
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .AsNoTracking() // TỐI ƯU: Tăng tốc độ hiển thị, giảm tải RAM cho Server
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // ==========================================
        // 2. CHI TIẾT ĐƠN HÀNG
        // ==========================================
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .AsNoTracking() // TỐI ƯU: Dành cho thao tác chỉ-đọc (Read-only)
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails!)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // ==========================================
        // 3. TÍNH NĂNG MỚI BỔ SUNG: CẬP NHẬT TRẠNG THÁI
        // Dành cho nút bấm [Duyệt đơn] / [Giao hàng] ở View
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken] // Chống tấn công giả mạo (CSRF)
        public async Task<IActionResult> UpdateStatus(int id, int status)
        {
            // Bỏ AsNoTracking ở đây vì chúng ta CẦN chỉnh sửa dữ liệu và lưu lại
            var order = await _context.Orders
                .Include(o => o.OrderDetails) // Thêm Include OrderDetails
                .Include(o => o.Customer)     // Thêm Include Customer để lấy Email
                .FirstOrDefaultAsync(o => o.Id == id);
                
            if (order == null) return NotFound();

            // Nếu trạng thái đổi thành "Đã Hủy" (3), thực hiện hoàn kho và gửi email
            if (status == 3 && order.Status != 3)
            {
                // 1. Hoàn kho
                foreach (var item in order.OrderDetails)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                    }
                }
                
                // 2. Gửi email thông báo hủy do hết hàng
                if (order.Customer != null && !string.IsNullOrEmpty(order.Customer.Email))
                {
                    string subject = $"TechZone - Đơn hàng #{order.Id} đã bị hủy do hết hàng";
                    string htmlMessage = $@"
                        <h3>Thông báo Hủy Đơn Hàng</h3>
                        <p>Xin chào <strong>{order.Customer.FullName}</strong>,</p>
                        <p>Thành thật xin lỗi quý khách, đơn hàng <strong>#{order.Id}</strong> của bạn đã bị hủy do một số sản phẩm trong đơn đã <strong>hết hàng</strong> tại kho.</p>
                        <p>Chúng tôi vô cùng xin lỗi vì sự bất tiện này và hy vọng sẽ được phục vụ quý khách ở những đơn hàng sau.</p>
                        <br/>
                        <p>Trân trọng,<br/>Đội ngũ TechZone</p>
                    ";
                    _ = _emailService.SendEmailAsync(order.Customer.Email, subject, htmlMessage).ContinueWith(t => {
                        if (t.IsFaulted) {
                            Console.WriteLine("Lỗi gửi email: " + t.Exception?.Message);
                        }
                    });
                }
            }

            // Cập nhật trạng thái mới
            order.Status = status;
            await _context.SaveChangesAsync();

            // Trả về thông báo thành công (View có thể dùng TempData để hiển thị popup)
            TempData["SuccessMessage"] = "Cập nhật trạng thái đơn hàng thành công!";

            // Chuyển hướng về lại đúng trang chi tiết của đơn hàng đó
            return RedirectToAction(nameof(Details), new { id = id });
        }
    }
}