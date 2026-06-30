using CMS.Backend.Models;
using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;

namespace CMS.Backend.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // ==========================================
        // 1. Giao diện cho người dùng (View của Backend)
        // ==========================================
        public async Task<IActionResult> Index()
        {
            // ĐÃ SỬA: Dùng CategoryPost và thêm AsNoTracking để tối ưu tốc độ đọc
            var latestPosts = await _context.Posts
                .Include(p => p.CategoryPost)
                .Where(p => p.IsPublished) // MỚI: Bắt buộc chỉ hiện bài đã xuất bản
                .OrderByDescending(p => p.CreatedDate)
                .Take(3)
                .AsNoTracking()
                .ToListAsync();

            return View(latestPosts);
        }

        // ==========================================
        // 2. API cho Frontend (React) gọi dữ liệu
        // Endpoint: https://localhost:7222/Home/GetLatestPostsApi
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetLatestPostsApi()
        {
            var latestPosts = await _context.Posts
                .Include(p => p.CategoryPost)
                .Where(p => p.IsPublished) // Tránh việc React kéo nhầm bản nháp
                .OrderByDescending(p => p.CreatedDate)
                .Take(3)
                .Select(p => new {
                    p.Id,
                    p.Title,
                    p.Slug, // MỚI: Trả về Slug để React tạo Link tĩnh chuẩn SEO
                    p.Summary, // MỚI: Trả về Sapo thay vì Content HTML nặng nề
                    p.ImageUrl,
                    p.CreatedDate,
                    p.ViewCount, // Trả thêm bộ đếm lượt xem
                    CategoryName = p.CategoryPost != null ? p.CategoryPost.Name : "Chưa phân loại"
                })
                .ToListAsync();

            // Dùng Ok() thay vì Json() để trả về HTTP Status 200 chuẩn RESTful API
            return Ok(latestPosts);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}