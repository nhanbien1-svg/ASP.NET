using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMS.Backend.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // DTOs
        /// <summary>
        /// Object chứa thông tin khi thêm hoặc sửa sản phẩm trong giỏ hàng
        /// </summary>
        public class CartItemRequest
        {
            /// <summary>Mã khách hàng</summary>
            public int CustomerId { get; set; }
            /// <summary>Mã sản phẩm</summary>
            public int ProductId { get; set; }
            /// <summary>Số lượng mua</summary>
            public int Quantity { get; set; }
        }

        // Lấy giỏ hàng của khách
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetCart(int customerId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart == null)
            {
                // Trả về mảng rỗng nếu chưa có giỏ
                return Ok(new List<object>());
            }

            var result = cart.CartItems.Select(ci => new
            {
                Id = ci.Id, // Id của CartItem
                ProductId = ci.ProductId,
                Name = ci.Product.Name,
                Price = ci.Product.Price,
                ImageUrl = ci.Product.ImageUrl,
                Quantity = ci.Quantity,
                StockQuantity = ci.Product.StockQuantity
            });

            return Ok(result);
        }

        /// <summary>
        /// Thêm sản phẩm vào giỏ hàng. Nếu sản phẩm đã có, sẽ tự động cộng dồn số lượng.
        /// </summary>
        /// <param name="request">Thông tin khách hàng, sản phẩm và số lượng</param>
        /// <returns>Thông báo thành công</returns>
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] CartItemRequest request)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId);

            if (cart == null)
            {
                cart = new Cart { CustomerId = request.CustomerId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync(); // Lưu để sinh CartId
            }

            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == request.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
            }
            else
            {
                cart.CartItems.Add(new CartItem
                {
                    CartId = cart.Id,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã thêm vào giỏ hàng" });
        }

        // Cập nhật số lượng
        [HttpPut("update")]
        public async Task<IActionResult> UpdateQuantity([FromBody] CartItemRequest request)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId);

            if (cart == null) return NotFound("Cart not found");

            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == request.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity = request.Quantity;
                if (existingItem.Quantity <= 0)
                {
                    _context.CartItems.Remove(existingItem);
                }
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Đã cập nhật số lượng" });
        }

        // Xóa 1 sản phẩm khỏi giỏ (dựa theo ProductId để dễ khớp với FrontEnd cũ)
        [HttpDelete("remove/{customerId}/{productId}")]
        public async Task<IActionResult> RemoveFromCart(int customerId, int productId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart != null)
            {
                var item = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
                if (item != null)
                {
                    _context.CartItems.Remove(item);
                    await _context.SaveChangesAsync();
                }
            }
            return Ok(new { message = "Đã xóa khỏi giỏ" });
        }

        // Làm sạch giỏ hàng
        [HttpDelete("clear/{customerId}")]
        public async Task<IActionResult> ClearCart(int customerId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart != null && cart.CartItems.Any())
            {
                _context.CartItems.RemoveRange(cart.CartItems);
                await _context.SaveChangesAsync();
            }
            return Ok(new { message = "Đã xóa sạch giỏ hàng" });
        }

        // Gộp giỏ hàng LocalStorage vào Backend
        [HttpPost("merge")]
        public async Task<IActionResult> MergeCart([FromBody] MergeRequest request)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId);

            if (cart == null)
            {
                cart = new Cart { CustomerId = request.CustomerId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            foreach (var localItem in request.LocalItems)
            {
                var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == localItem.ProductId);
                if (existingItem != null)
                {
                    existingItem.Quantity += localItem.Quantity;
                }
                else
                {
                    cart.CartItems.Add(new CartItem
                    {
                        CartId = cart.Id,
                        ProductId = localItem.ProductId,
                        Quantity = localItem.Quantity
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã đồng bộ giỏ hàng" });
        }

        public class MergeRequest
        {
            public int CustomerId { get; set; }
            public List<LocalCartItem> LocalItems { get; set; }
        }

        public class LocalCartItem
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }
    }
}
