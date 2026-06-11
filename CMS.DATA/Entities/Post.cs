using System;
using System.ComponentModel.DataAnnotations;

namespace CMS.Data.Entities
{
    public class Post
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống.")]
        public string Content { get; set; }

        // Cho phép ImageUrl là null bằng cách thêm dấu ?
        public string? ImageUrl { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Khóa ngoại
        [Required(ErrorMessage = "Vui lòng chọn chuyên mục.")]
        public int CategoryId { get; set; }

        // Navigation Property: Nên để virtual để EF Core hỗ trợ Lazy Loading
        public virtual Category? Category { get; set; }
    }
}