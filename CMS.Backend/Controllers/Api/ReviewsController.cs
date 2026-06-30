using CMS.Backend.DTOs;
using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/reviews/product/{productId}
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetProductReviews(int productId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.Customer)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedDate)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    ProductId = r.ProductId,
                    CustomerId = r.CustomerId,
                    CustomerName = r.Customer.FullName,
                    CustomerAvatar = r.Customer.AvatarUrl,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedDate = r.CreatedDate
                })
                .ToListAsync();

            return Ok(reviews);
        }

        // POST: api/reviews
        [HttpPost]
        public async Task<ActionResult<ReviewDto>> CreateReview([FromBody] CreateReviewDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Có thể bổ sung kiểm tra xem user đã mua hàng chưa ở đây (Tương lai)

            var review = new Review
            {
                ProductId = createDto.ProductId,
                CustomerId = createDto.CustomerId,
                Rating = createDto.Rating,
                Comment = createDto.Comment,
                CreatedDate = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Trả về DTO
            var customer = await _context.Customers.FindAsync(createDto.CustomerId);
            var resultDto = new ReviewDto
            {
                Id = review.Id,
                ProductId = review.ProductId,
                CustomerId = review.CustomerId,
                CustomerName = customer?.FullName ?? "Khách hàng",
                CustomerAvatar = customer?.AvatarUrl,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedDate = review.CreatedDate
            };

            return CreatedAtAction(nameof(GetProductReviews), new { productId = review.ProductId }, resultDto);
        }
    }
}
