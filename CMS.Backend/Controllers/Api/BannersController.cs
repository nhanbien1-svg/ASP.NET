using CMS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CMS.Backend.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class BannersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BannersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetBanners()
        {
            var banners = await _context.Banners
                .Where(b => b.IsActive)
                .OrderBy(b => b.DisplayOrder)
                .Select(b => new {
                    b.Id,
                    b.Title,
                    b.Badge,
                    b.Description,
                    b.Price,
                    b.ImageUrl,
                    b.ThemeColor,
                    b.DisplayOrder
                })
                .ToListAsync();

            return Ok(banners);
        }
    }
}
