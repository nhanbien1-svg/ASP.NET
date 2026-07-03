using System;
using System.ComponentModel.DataAnnotations;

namespace CMS.Data.Entities
{
    public class Banner
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(200)]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }

        [StringLength(100)]
        [Display(Name = "Nhãn nổi bật (Badge)")]
        public string? Badge { get; set; }

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [StringLength(100)]
        [Display(Name = "Giá/Thông điệp")]
        public string? Price { get; set; }

        [StringLength(500)]
        [Display(Name = "Đường dẫn ảnh")]
        public string? ImageUrl { get; set; }

        [StringLength(20)]
        [Display(Name = "Mã màu chủ đạo (Theme)")]
        public string? ThemeColor { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Thứ tự hiển thị")]
        public int DisplayOrder { get; set; } = 0;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
