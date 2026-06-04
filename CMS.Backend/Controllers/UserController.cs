using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // 1. BẮT BUỘC CÓ ĐỂ DÙNG BẢO MẬT
using CMS.Data;
using CMS.DATA.Entities;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace CMS.Backend.Controllers
{
    [Authorize] // 2. KHÓA TOÀN BỘ CONTROLLER: Chỉ người đã đăng nhập mới truy cập được
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. GET: Danh sách thành viên
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // 2. GET: Form tạo mới
        public IActionResult Create() => View();

        // 3. POST: Xử lý lưu thành viên mới
        [HttpPost]
        [ValidateAntiForgeryToken] // Chống tấn công giả mạo (CSRF)
        public async Task<IActionResult> Create(User user, string Password)
        {
            if (await _context.Users.AnyAsync(u => u.UserName == user.UserName))
            {
                ViewBag.Error = "Tên đăng nhập này đã tồn tại!";
                return View(user);
            }

            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                ViewBag.Error = "Địa chỉ Email này đã được sử dụng!";
                return View(user);
            }

            if (string.IsNullOrEmpty(Password))
            {
                ViewBag.Error = "Vui lòng nhập mật khẩu!";
                return View(user);
            }

            ModelState.Remove("PasswordHash");

            if (ModelState.IsValid)
            {
                var hasher = new PasswordHasher<User>();
                user.PasswordHash = hasher.HashPassword(user, Password);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Error = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return View(user);
        }

        // 4. GET: Form sửa thông tin
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // 5. POST: Xử lý cập nhật thông tin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User model, string NewPassword)
        {
            var existingUser = await _context.Users.FindAsync(model.Id);
            if (existingUser == null) return NotFound();

            ModelState.Remove("PasswordHash");

            if (ModelState.IsValid)
            {
                existingUser.UserName = model.UserName;
                existingUser.FullName = model.FullName;
                existingUser.Email = model.Email;
                existingUser.Role = model.Role;

                if (!string.IsNullOrEmpty(NewPassword))
                {
                    var hasher = new PasswordHasher<User>();
                    existingUser.PasswordHash = hasher.HashPassword(existingUser, NewPassword);
                }

                _context.Users.Update(existingUser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Error = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return View(model);
        }

        // 6. POST: Xóa thành viên (CHỈ ADMIN MỚI ĐƯỢC PHÉP)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // 3. PHÂN QUYỀN: Chỉ Admin mới xóa được thành viên
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}