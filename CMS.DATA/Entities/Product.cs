using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên thiết bị/sản phẩm không được để trống")]
        [StringLength(200, ErrorMessage = "Tên sản phẩm không được vượt quá 200 ký tự")]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Range(0, 999999999999.99, ErrorMessage = "Giá bán phải là số dương hợp lệ")]
        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho không được là số âm")]
        public int StockQuantity { get; set; }

        public string? ImageUrl { get; set; }

        // ======================================================
        // CÁC TRƯỜNG DỮ LIỆU BỔ SUNG CHO E-COMMERCE CHUYÊN NGHIỆP
        // ======================================================

        [Display(Name = "Ngày nhập hệ thống")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Hiển thị trên Web")]
        public bool IsActive { get; set; } = true; // Mặc định tạo ra là được hiển thị luôn

        // ======================================================
        // QUAN HỆ KHÓA NGOẠI VỚI CÂY DANH MỤC
        // ======================================================

        [Required(ErrorMessage = "Vui lòng chọn danh mục cho sản phẩm")]
        [Display(Name = "Danh mục trực thuộc")]
        public int CategoryProductId { get; set; }

        [ForeignKey("CategoryProductId")]
        public virtual CategoryProduct? CategoryProduct { get; set; }

        public virtual ICollection<Review>? Reviews { get; set; }
    }
}