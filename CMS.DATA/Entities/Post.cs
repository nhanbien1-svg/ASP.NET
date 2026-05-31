using CMS.DATA.Entities;
using System;

namespace CMS.Data.Entities // 1. Đã sửa DATA thành Data cho khớp với Controller
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now; // 2. Đã sửa CreatedData thành CreatedDate

        // Khóa ngoại kết nối với Category
        public int CategoryId { get; set; } // 3. Đã sửa CategoryID thành CategoryId
        public virtual Category Category { get; set; }
    }
}