/*
 * Sinh vien
 * Ma sinh vien:
 * Lop ccq2311E
 * Ngay tao:
 * Mo ta
 * Damh muc
 
 
 
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
