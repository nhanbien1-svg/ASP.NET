using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using Microsoft.AspNetCore.Authorization;

namespace CMS.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous] // BẮT BUỘC: Cho phép React truy cập công khai không cần đăng nhập
    public class PostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==================================================
        // 1. DÀNH CHO TRANG BLOG (LẤY TẤT CẢ BÀI VIẾT)
        // ==================================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var posts = await _context.Posts
                .AsNoTracking()
                .Where(p => p.IsPublished)
                .OrderByDescending(p => p.CreatedDate)
                .Select(p => new {
                    p.Id,
                    p.Title,
                    p.Slug,
                    p.Summary,
                    p.ImageUrl,
                    p.CreatedDate,
                    p.ViewCount,
                    CategoryPostId = p.CategoryPostId,
                    // TỐI ƯU CÚ PHÁP: Sử dụng toán tử ?? thay cho toán tử 3 ngôi
                    CategoryName = p.CategoryPost.Name ?? "Chưa phân loại"
                })
                .ToListAsync();

            return Ok(posts);
        }

        // ==================================================
        // 2. DÀNH CHO TRANG CHỦ (LẤY 3 BÀI MỚI NHẤT)
        // ==================================================
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatest()
        {
            var posts = await _context.Posts
                .AsNoTracking()
                .Where(p => p.IsPublished)
                .OrderByDescending(p => p.CreatedDate)
                .Take(3)
                .Select(p => new {
                    p.Id,
                    p.Title,
                    p.Slug,
                    p.Summary,
                    p.ImageUrl,
                    p.CreatedDate,
                    p.ViewCount,
                    CategoryPostId = p.CategoryPostId,
                    CategoryName = p.CategoryPost.Name ?? "Chưa phân loại"
                })
                .ToListAsync();

            return Ok(posts);
        }

        // ==================================================
        // 3. DÀNH CHO TRANG CHI TIẾT BÀI VIẾT (TÌM THEO ID)
        // ==================================================
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var post = await _context.Posts
                .AsNoTracking()
                .Where(p => p.IsPublished)
                // Ép kiểu Select ngay từ đầu để EF Core không cần tải dữ liệu rác về RAM
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Slug,
                    p.Summary,
                    p.Content, // Lấy nội dung HTML
                    p.ImageUrl,
                    p.CreatedDate,
                    p.ViewCount,
                    CategoryName = p.CategoryPost.Name ?? "Chưa phân loại"
                })
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound(new { message = "Không tìm thấy bài viết hoặc bài viết đang bị ẩn." });

            return Ok(post);
        }

        // ==================================================
        // 4. [TÍNH NĂNG PRO] TÌM BÀI VIẾT THEO SLUG (CHUẨN SEO)
        // ==================================================
        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var post = await _context.Posts
                .AsNoTracking()
                .Where(p => p.IsPublished)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Slug,
                    p.Summary,
                    p.Content,
                    p.ImageUrl,
                    p.CreatedDate,
                    p.ViewCount,
                    CategoryName = p.CategoryPost.Name ?? "Chưa phân loại"
                })
                .FirstOrDefaultAsync(p => p.Slug == slug);

            if (post == null) return NotFound(new { message = "Không tìm thấy bài viết." });

            return Ok(post);
        }
    }
}