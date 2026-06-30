/*
 * Sinh viên: Biện Văn Nhân
 * Mã sinh viên: 2123110177
 * Lớp: CCQ2311E
 * Ngày tạo: 15/05/2026
 * Mô tả: Thực thể Bài viết (Post) - Đã nâng cấp chuẩn SEO và Đồng bộ khóa ngoại
 */
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề bài viết không được để trống.")]
        [StringLength(300, ErrorMessage = "Tiêu đề không được vượt quá 300 ký tự.")]
        public string Title { get; set; }

        // MỚI: Đường dẫn tĩnh SEO (VD: huong-dan-mua-laptop-2026)
        [Display(Name = "Đường dẫn tĩnh (SEO)")]
        [StringLength(350)]
        public string? Slug { get; set; }

        // MỚI: Đoạn văn ngắn tóm tắt bài viết hiển thị ở trang chủ
        [Display(Name = "Mô tả ngắn (Sapo)")]
        [StringLength(500, ErrorMessage = "Mô tả ngắn không được vượt quá 500 ký tự.")]
        public string? Summary { get; set; }

        [Required(ErrorMessage = "Nội dung bài viết không được để trống.")]
        public string Content { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public string? ImageUrl { get; set; }

        // MỚI: Bộ đếm lượt xem bài viết
        [Display(Name = "Lượt xem")]
        public int ViewCount { get; set; } = 0;

        [Display(Name = "Ngày đăng tải")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // MỚI: Trạng thái bài viết (Lưu nháp / Đã xuất bản)
        [Display(Name = "Trạng thái xuất bản")]
        public bool IsPublished { get; set; } = true;

        // ==========================================
        // QUAN HỆ KHÓA NGOẠI VỚI BẢNG CATEGORY_POST
        // ==========================================

        [Required(ErrorMessage = "Vui lòng chọn chuyên mục tin tức.")]
        [Display(Name = "Chuyên mục trực thuộc")]
        public int CategoryPostId { get; set; } // ĐÃ SỬA: Đổi từ CategoryId thành CategoryPostId cho khớp với thực thể Cha

        // Navigation Property: Nên để virtual để EF Core hỗ trợ Lazy Loading
        [ForeignKey("CategoryPostId")]
        public virtual CategoryPost? CategoryPost { get; set; } // ĐÃ SỬA: Đổi từ Category thành CategoryPost
    }
}