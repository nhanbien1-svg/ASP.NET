using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data; // Thư mục chứa DbContext của dự án
using CMS.Data.Entities; // Thư mục chứa thực thể Post và Category
using System.Linq;

namespace CMS.Controllers
{
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Hàm khởi tạo - Tiêm DbContext vào để làm việc với cơ sở dữ liệu
        public PostController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 2.1. TRANG DANH SÁCH BÀI VIẾT (TẤT CẢ HOẶC LỌC THEO DANH MỤC)
        // URL Toàn bộ bài viết: https://localhost:xxxx/Post
        // URL Lọc theo danh mục: https://localhost:xxxx/Post/Index/5
        // ==========================================
        public IActionResult Index(int? id)
        {
            // Bước 1: Khởi tạo câu lệnh truy vấn nạp kèm bảng Category (Eager Loading)
            var query = _context.Posts.Include(p => p.Category).AsQueryable();

            // Bước 2: Xử lý logic thông minh linh hoạt cho ID
            if (id.HasValue)
            {
                // CÓ ID: Tiến hành lọc các bài viết thuộc danh mục này
                query = query.Where(p => p.CategoryId == id.Value);

                // Lấy tên danh mục gán vào ViewBag để hiển thị tiêu đề ngoài giao diện cho đẹp
                var currentCategory = _context.Categories.Find(id.Value);
                ViewBag.TitlePage = currentCategory != null ? $"Danh mục: {currentCategory.Name}" : "Danh mục không tồn tại";
            }
            else
            {
                // KHÔNG CÓ ID: Hiển thị toàn bộ bài viết hệ thống có
                ViewBag.TitlePage = "Tất cả bài viết";
            }

            // Bước 3: Sắp xếp thời gian giảm dần (mới nhất lên đầu) và thực thi lấy dữ liệu
            var posts = query.OrderByDescending(p => p.CreatedDate).ToList();

            // Bước 4: Đẩy danh sách ra View hiển thị
            return View(posts);
        }

        // ==========================================
        // 3.1. TRANG XEM CHI TIẾT BÀI VIẾT (DETAILS)
        // URL ví dụ: https://localhost:xxxx/Post/Details/1
        // ==========================================
        public IActionResult Details(int id)
        {
            // Bước 1: Tìm bài viết duy nhất theo ID, lôi kèm thông tin Danh mục của bài đó
            var post = _context.Posts
                               .Include(p => p.Category)
                               .FirstOrDefault(p => p.Id == id);

            // Bước 2: Kiểm tra bảo vệ hệ thống nếu ID bừa bãi không tồn tại
            if (post == null)
            {
                return NotFound();
            }

            // Bước 3: Truyền bài viết tìm được sang giao diện Details
            return View(post);
        }
    }
}