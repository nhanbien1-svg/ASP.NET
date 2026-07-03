using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Mã khách hàng không được để trống")]
        public int CustomerId { get; set; }

        // --- CÁC TRƯỜNG ĐÃ BỔ SUNG ĐỂ LƯU LỊCH SỬ ĐƠN HÀNG ---

        [Required(ErrorMessage = "Người nhận không được để trống")]
        [MaxLength(100)]
        public string ShippingName { get; set; } // Tên người nhận (có thể khác tên người mua)

        [Required(ErrorMessage = "Số điện thoại giao hàng không được để trống")]
        [MaxLength(20)]
        public string ShippingPhone { get; set; }

        [Required(ErrorMessage = "Địa chỉ giao hàng không được để trống")]
        [MaxLength(255)]
        public string ShippingAddress { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")] // Ép kiểu tiền tệ chuẩn xác trong SQL Server
        public decimal TotalAmount { get; set; } // TỔNG TIỀN ĐƠN HÀNG (Bắt buộc phải lưu cứng)

        // ----------------------------------------------------

        public int Status { get; set; } = 0;

        [Required(ErrorMessage = "Phương thức thanh toán không được để trống")]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = "COD"; // Phương thức thanh toán (COD, VNPAY, MOMO, v.v.)

        [MaxLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Notes { get; set; }

        [MaxLength(500)]
        [Display(Name = "Lý do hủy đơn")]
        public string? CancelReason { get; set; }

        // --- QUAN HỆ CƠ SỞ DỮ LIỆU ---

        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        public virtual ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}