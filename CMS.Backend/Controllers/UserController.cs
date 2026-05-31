using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data; // Đảm bảo namespace chứa ApplicationDbContext
using CMS.DATA.Entities; // Namespace chứa thực thể User của bạn
using Microsoft.AspNetCore.Identity; // Cần thiết để sử dụng PasswordHasher
using System.Linq;
using System.Threading.Tasks;

namespace CMS.Backend.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Tiêm (Inject) DbContext vào Controller
        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH (Index)
        public async Task<IActionResult> Index()
        {
            // Lấy dữ liệu thật từ Database
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // 2. FORM THÊM MỚI (GET)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 3. XỬ LÝ LƯU DỮ LIỆU (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra trùng lặp UserName (tránh lỗi khóa chính hoặc logic)
                if (_context.Users.Any(u => u.UserName == user.UserName))
                {
                    ModelState.AddModelError("UserName", "Tên đăng nhập này đã tồn tại!");
                    return View(user);
                }

                // MÃ HÓA MẬT KHẨU (Bắt buộc để bảo mật)
                // PasswordHasher sẽ chuyển mật khẩu dạng text thành chuỗi mã hóa an toàn
                var hasher = new PasswordHasher<User>();
                user.PasswordHash = hasher.HashPassword(user, user.PasswordHash);

                // Lưu vào Database
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Nếu model không hợp lệ, trả về form kèm lỗi
            return View(user);
        }
    }
}