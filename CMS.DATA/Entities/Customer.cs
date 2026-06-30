using System.ComponentModel.DataAnnotations;

namespace CMS.Data.Entities
{
    // Khách hàng (Dành cho phân hệ E-Commerce)
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [MaxLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(20, ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? Phone { get; set; }

        [MaxLength(255, ErrorMessage = "Địa chỉ quá dài, vui lòng rút gọn")]
        public string? Address { get; set; }

        public string? AvatarUrl { get; set; }



        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string PasswordHash { get; set; } // ĐÃ ĐỔI: Phải băm mật khẩu để bảo mật

        // --- CÁC TRƯỜNG BỔ SUNG CHUẨN QUẢN TRỊ ---
        public bool IsActive { get; set; } = true; // Mặc định khách tạo xong là được hoạt động

        public DateTime CreatedDate { get; set; } = DateTime.Now; // Lưu vết ngày khách hàng đăng ký

        // --- QUAN HỆ CƠ SỞ DỮ LIỆU ---
        public virtual ICollection<Order>? Orders { get; set; }
        public virtual ICollection<Review>? Reviews { get; set; }
    }
}