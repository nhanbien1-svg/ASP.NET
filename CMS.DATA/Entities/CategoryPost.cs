/*
 * Sinh viên: Biện Văn Nhân
 * Mã sinh viên: 2123110177
 * Lớp: CCQ2311E
 * Ngày tạo: 15/05/2026
 * Mô tả: Thực thể Danh mục Tin tức (CategoryPost) - Đã nâng cấp chuẩn SEO, Phân cấp Cha-Con và Validation
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities // Đã chuẩn hóa viết hoa viết thường
{
    public class CategoryPost // Đổi tên để tránh xung đột với CategoryProduct
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [StringLength(200, ErrorMessage = "Tên danh mục không được vượt quá 200 ký tự")]
        public string Name { get; set; }

        // MỚI: Đường dẫn tĩnh chuẩn SEO (Ví dụ: tin-cong-nghe)
        [StringLength(255)]
        public string? Slug { get; set; }

        public string? Description { get; set; }

        // ==========================================
        // TÍNH NĂNG NÂNG CAO (SEO & HIỂN THỊ)
        // ==========================================

        [Display(Name = "Trạng thái hiển thị")]
        public bool IsActive { get; set; } = true; // Mặc định tạo ra là hiển thị luôn

        [Display(Name = "Danh mục cha")]
        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public virtual CategoryPost? ParentCategory { get; set; }

        // ==========================================
        // QUAN HỆ VỚI BẢNG BÀI VIẾT (POST)
        // ==========================================

        // Một danh mục tin tức có thể chứa nhiều bài viết
        // Dấu '?' giúp tránh cảnh báo Nullable reference types trong .NET mới
        public virtual ICollection<Post>? Posts { get; set; }
    }
}