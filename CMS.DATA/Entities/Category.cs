/*
 * Sinh viên: Biện Văn Nhân
 * Mã sinh viên:2123110177
 * Lớp CCQ2311E
 * Ngày tạo:15/05/2026
 * Mô tả:
 * Danh mục:
 
 
 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.DATA.Entities
{
   public class Category
    {
       
        
        public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        // Quan hệ danh mục có nhiều bài viết
        public virtual ICollection<Category> Categories { get; set; }


        
    }
}
