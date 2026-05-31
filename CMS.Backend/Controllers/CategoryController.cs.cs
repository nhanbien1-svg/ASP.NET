using CMS.Data;                      // Kết nối tới ApplicationDbContext
using CMS.Data.Entities;             // Kết nối tới thực thể Category
using CMS.DATA.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Giúp hỗ trợ các tính năng nâng cao của Entity Framework
using System.Linq;

namespace CMS.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        // "Tiêm" kết nối Cơ sở dữ liệu vào Controller
        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. TRANG DANH SÁCH DANH MỤC (R - Read)
        public IActionResult Index()
        {
            // Lấy dữ liệu THẬT từ bảng Categories trong SQL
            var data = _context.Categories.ToList();
            return View(data);
        }

        // 2. THÊM DANH MỤC (C - Create: Hàm GET để hiển thị Form nhập liệu trống)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 3. THÊM DANH MỤC (C - Create: Hàm POST để đón dữ liệu từ Form gửi lên và lưu vào SQL)
        [HttpPost]
        public IActionResult Create(Category model)
        {
            // Bước 1: Thêm dữ liệu vào bộ nhớ tạm
            _context.Categories.Add(model);

            // Bước 2: Chốt phiên làm việc, lưu thực sự xuống SQL Server
            _context.SaveChanges();

            // Lưu xong, tự động điều hướng quay lại trang danh sách
            return RedirectToAction("Index");
        }

        // 4. CHỈNH SỬA DANH MỤC (U - Update: Hàm GET tìm dữ liệu cũ và đổ lên Form)
        [HttpGet]
        public IActionResult Edit(int id)
        {
            // Tìm dòng danh mục trong Database theo đúng Id truyền vào
            var category = _context.Categories.Find(id);

            // Nếu không tìm thấy (Id linh tinh), trả về trang lỗi 404
            if (category == null)
            {
                return NotFound();
            }

            // Gửi đối tượng tìm được sang giao diện trang Edit.cshtml để hiển thị thông tin cũ
            return View(category);
        }

        // 5. CHỈNH SỬA DANH MỤC (U - Update: Hàm POST nhận dữ liệu mới đã sửa từ người dùng)
        [HttpPost]
        public IActionResult Edit(Category model)
        {
            // Cập nhật thông tin đối tượng vào bộ nhớ tạm
            _context.Categories.Update(model);

            // Lưu thay đổi thực sự xuống SQL Server
            _context.SaveChanges();

            // Quay lại trang danh sách để xem kết quả cập nhật
            return RedirectToAction("Index");
        }

        // 6. XÓA DANH MỤC (D - Delete: Nhận Id cần xóa)
        public IActionResult Delete(int id)
        {
            // Bước 1: Tìm đối tượng cần xóa trong Database
            var category = _context.Categories.Find(id);

            // Kiểm tra nếu tồn tại dữ liệu thì tiến hành xóa
            if (category != null)
            {
                // Bước 2: Đánh dấu xóa khỏi bộ nhớ tạm
                _context.Categories.Remove(category);

                // Bước 3: Thực thi câu lệnh DELETE thực sự trong SQL Server
                _context.SaveChanges();
            }

            // Quay lại trang danh sách để làm mới giao diện
            return RedirectToAction("Index");
        }
    }
}