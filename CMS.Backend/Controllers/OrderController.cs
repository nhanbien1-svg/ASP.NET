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

        public OrderController(ApplicationDbContext context) => _context = context;

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
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

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