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
namespace CMS.DATA.Entities
{
    public class Post 
    { 
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content {  get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedData { get; set; } = DateTime.Now;
        // Khóa ngoại kết nối với Category

        public int CategoryID { get; set; }
        public virtual Category Category { get; set; }


    }
}
namespace CMS.DATA.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; } // Quản trị viên hoặc biên tập viên
    }
}

namespace CMS.DATA.Entities
{
    public class CategoryProduct

    {

    }
}
