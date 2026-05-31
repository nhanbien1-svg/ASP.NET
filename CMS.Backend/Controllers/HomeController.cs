using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Bắt buộc phải có để dùng .Include()
using System.Linq;                   // Bắt buộc phải có để dùng .OrderByDescending() và .Take()
using CMS.Backend.Models;
using CMS.Data;                      // Thư mục chứa ApplicationDbContext
using CMS.Data.Entities;             // Thư mục chứa thực thể Post, Category

namespace CMS.Backend.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        // 1. Khai báo biến DbContext để kết nối Database
        private readonly ApplicationDbContext _context;

        // 2. Hàm khởi tạo: Tiêm cả Logger và DbContext vào Controller
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // ==========================================
        // YÊU CẦU 2: HIỂN THỊ 3 BÀI VIẾT MỚI NHẤT
        // ==========================================
        public IActionResult Index()
        {
            // LINQ: Tiến hành nối bảng, sắp xếp giảm dần theo ngày và lấy đúng 3 bài
            var latestPosts = _context.Posts
                                      .Include(p => p.Category) // Lấy kèm Tên danh mục
                                      .OrderByDescending(p => p.CreatedDate) // Mới nhất lên đầu
                                      .Take(3) // Lấy 3 bản tin đầu tiên
                                      .ToList();

            // Truyền danh sách 3 bài viết ra ngoài giao diện Index.cshtml
            return View(latestPosts);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}