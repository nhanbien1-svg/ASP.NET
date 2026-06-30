using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities
{
    public class CategoryProduct
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [StringLength(100)]
        public string Name { get; set; }

        public string? Description { get; set; }

        // MỚI THÊM: Đường dẫn ảnh logo/thương hiệu của danh mục
        public string? ImageUrl { get; set; }

        // Cấu trúc cây danh mục cha - con
        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public virtual CategoryProduct? ParentCategory { get; set; }
        public virtual ICollection<CategoryProduct>? SubCategories { get; set; }
        public virtual ICollection<Product>? Products { get; set; }
    }
}