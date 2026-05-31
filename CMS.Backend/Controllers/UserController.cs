using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using CMS.DATA.Entities;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace CMS.Backend.Controllers
{
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Users.AnyAsync(u => u.UserName == user.UserName))
                {
                    ModelState.AddModelError("UserName", "Tên đăng nhập này đã tồn tại!");
                    return View(user);
                }

                var hasher = new PasswordHasher<User>();
                user.PasswordHash = hasher.HashPassword(user, user.PasswordHash);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
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
            var existingUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == model.Id);

            if (existingUser == null) return NotFound();

            if (!string.IsNullOrEmpty(NewPassword))
            {
                var hasher = new PasswordHasher<User>();
                model.PasswordHash = hasher.HashPassword(model, NewPassword);
            }
            else
            {
                model.PasswordHash = existingUser.PasswordHash;
            }

            _context.Users.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // 6. POST: Xóa thành viên (Bắt buộc dùng POST để bảo mật)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
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