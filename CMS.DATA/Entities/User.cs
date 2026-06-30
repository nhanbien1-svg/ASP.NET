using System;
using System.ComponentModel.DataAnnotations;

namespace CMS.Data.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3 đến 50 ký tự")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Họ và tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ")]
        [StringLength(150, ErrorMessage = "Email không được vượt quá 150 ký tự")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vai trò không được để trống")]
        [StringLength(20)]
        public string Role { get; set; } = "Admin";

        [Required]
        // Không giới hạn độ dài ở đây vì chuỗi Hash sinh ra thường khá dài (tùy thuộc thuật toán như BCrypt)
        public string PasswordHash { get; set; }

        // ==========================================
        // CÁC TRƯỜNG QUẢN TRỊ NÂNG CAO
        // ==========================================

        [Display(Name = "Trạng thái hoạt động")]
        public bool IsActive { get; set; } = true; // Dùng để khóa tài khoản (Ban user) thay vì xóa

        [Display(Name = "Ngày tạo tài khoản")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}