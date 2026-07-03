namespace CMS.Backend.DTOs
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string? CustomerAvatar { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool HasBought { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class CreateReviewDto
    {
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public Microsoft.AspNetCore.Http.IFormFile? ImageFile { get; set; }
    }
}
