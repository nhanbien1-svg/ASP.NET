using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        // Ràng buộc số lượng không được âm hoặc bằng 0
        [Required]
        [Range(1, 10000, ErrorMessage = "Số lượng mua phải lớn hơn 0")]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } // Giá chốt tại thời điểm mua

        // --- TRƯỜNG ẢO ---
        // Giúp View giao diện tự động tính "Thành tiền" của từng món mà không cần lưu xuống Database
        [NotMapped]
        public decimal SubTotal => Quantity * UnitPrice;

        // --- QUAN HỆ CƠ SỞ DỮ LIỆU ---
        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}