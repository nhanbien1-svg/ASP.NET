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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ReviewsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: api/reviews/product/{productId}
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetProductReviews(int productId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.Customer)
                .Where(r => r.ProductId == productId && r.IsApproved)
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
                    CreatedDate = r.CreatedDate,
                    ImageUrl = r.ImageUrl,
                    HasBought = _context.Orders.Any(o => o.CustomerId == r.CustomerId && o.OrderDetails!.Any(od => od.ProductId == r.ProductId))
                })
                .ToListAsync();

            return Ok(reviews);
        }

        // POST: api/reviews
        [HttpPost]
        public async Task<ActionResult<ReviewDto>> CreateReview([FromForm] CreateReviewDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string? imageUrl = null;
            if (createDto.ImageFile != null && createDto.ImageFile.Length > 0)
            {
                string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "reviews");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(createDto.ImageFile.FileName);
                string filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await createDto.ImageFile.CopyToAsync(stream);
                }
                imageUrl = "/images/reviews/" + fileName;
            }

            var review = new Review
            {
                ProductId = createDto.ProductId,
                CustomerId = createDto.CustomerId,
                Rating = createDto.Rating,
                Comment = createDto.Comment,
                ImageUrl = imageUrl,
                IsApproved = true, // Mặc định duyệt luôn
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
                CreatedDate = review.CreatedDate,
                ImageUrl = review.ImageUrl,
                HasBought = await _context.Orders.AnyAsync(o => o.CustomerId == review.CustomerId && o.OrderDetails!.Any(od => od.ProductId == review.ProductId))
            };

            return CreatedAtAction(nameof(GetProductReviews), new { productId = review.ProductId }, resultDto);
        }
    }
}
