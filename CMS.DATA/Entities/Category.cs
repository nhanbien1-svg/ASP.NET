/*
 * Sinh viên: Biện Văn Nhân
 * Mã sinh viên: 2123110177
 * Lớp CCQ2311E
 * Ngày tạo: 15/05/2026
 * Mô tả: Thực thể Danh mục (Category) - Đã sửa lỗi Namespace và Quan hệ
 */
using System;
using System.Collections.Generic;

namespace CMS.Data.Entities // 🔴 LỖI 1: Đã sửa DATA thành Data (viết thường chữ 'ata')
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        // 🟢 LỖI 2: Một danh mục có nhiều bài viết (Sửa từ Category -> Post)
        // (Nếu file bài viết của bạn đặt tên khác Post, hãy đổi chữ Post này thành tên file đó nhé)
        public virtual ICollection<Post> Posts { get; set; }
    }
}