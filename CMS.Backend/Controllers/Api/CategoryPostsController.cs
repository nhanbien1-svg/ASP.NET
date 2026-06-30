using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;

namespace CMS.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous] // BẮT BUỘC: Cho phép React truy cập công khai không cần đăng nhập
    public class CategoryPostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoryPostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==================================================
        // LẤY TẤT CẢ DANH MỤC BÀI VIẾT
        // ==================================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _context.CategoryPosts
                .AsNoTracking()
                .OrderBy(c => c.Name) // Sửa lỗi ở đây: OrderBy(Name) thay vì DisplayOrder
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Slug,
                    c.IsActive
                })
                .ToListAsync();

            return Ok(categories);
        }
    }
}
