using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using CMS.Data;
using CMS.Data.Entities;

namespace CMS.Backend.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")] // Phân quyền cấp cao nhất cho Controller này
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _hasher;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
            _hasher = new PasswordHasher<User>();
        }

        // ==========================================
        // 1. GET: Danh sách thành viên
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.AsNoTracking().ToListAsync();
            return View(users);
        }

        // ==========================================
        // 2. GET: Form tạo mới
        // ==========================================
        public IActionResult Create() => View();

        // ==========================================
        // 3. POST: Xử lý lưu thành viên mới
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user, string Password)
        {
            // BẮT BUỘC: Loại bỏ kiểm tra PasswordHash vì ta sẽ tự băm (hash) ở dưới
            ModelState.Remove("PasswordHash");

            // Kiểm tra mật khẩu rỗng
            if (string.IsNullOrWhiteSpace(Password))
                ModelState.AddModelError("Password", "Mật khẩu không được để trống!");

            // Kiểm tra trùng lặp UserName & Email
            if (await _context.Users.AnyAsync(u => u.UserName == user.UserName))
                ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại!");

            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                ModelState.AddModelError("Email", "Địa chỉ Email này đã được sử dụng!");

            if (ModelState.IsValid)
            {
                // Băm mật khẩu và lưu
                user.PasswordHash = _hasher.HashPassword(user, Password);
                user.CreatedDate = DateTime.Now; // Gán ngày tạo tự động
                // Trạng thái IsActive sẽ được lấy từ checkbox trên form Create

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm thành viên mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // ==========================================
        // 4. GET: Form sửa thông tin
        // ==========================================
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // ==========================================
        // 5. POST: Xử lý cập nhật thông tin
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User model, string? NewPassword)
        {
            var existingUser = await _context.Users.FindAsync(model.Id);
            if (existingUser == null) return NotFound();

            // BẮT BUỘC: Loại bỏ kiểm tra PasswordHash
            ModelState.Remove("PasswordHash");

            // Kiểm tra trùng lặp (NHƯNG BỎ QUA user hiện tại đang sửa)
            if (await _context.Users.AnyAsync(u => u.UserName == model.UserName && u.Id != model.Id))
                ModelState.AddModelError("UserName", "Tên đăng nhập đã có người khác sử dụng!");

            if (await _context.Users.AnyAsync(u => u.Email == model.Email && u.Id != model.Id))
                ModelState.AddModelError("Email", "Email này đã có người khác sử dụng!");

            if (ModelState.IsValid)
            {
                // Cập nhật các trường cơ bản
                existingUser.UserName = model.UserName;
                existingUser.FullName = model.FullName;
                existingUser.Email = model.Email;
                // Tránh trường hợp tự hạ quyền hoặc tự khóa tài khoản của chính mình
                if (existingUser.UserName != User.Identity?.Name)
                {
                    existingUser.Role = model.Role;
                    existingUser.IsActive = model.IsActive;
                }
                if (!string.IsNullOrWhiteSpace(NewPassword))
                {
                    existingUser.PasswordHash = _hasher.HashPassword(existingUser, NewPassword);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật thông tin thành viên thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // ==========================================
        // 6. GET: Hiển thị trang xác nhận xóa
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userToDelete = await _context.Users.FindAsync(id);
            if (userToDelete == null) return NotFound();

            // Bảo mật: Không cho phép tự vào trang xóa chính mình
            var currentUser = User.Identity?.Name;
            if (userToDelete.UserName == currentUser)
            {
                TempData["Error"] = "Hệ thống từ chối: Bạn không thể tự xóa tài khoản của chính mình!";
                return RedirectToAction(nameof(Index));
            }

            return View(userToDelete);
        }

        // ==========================================
        // 7. POST: Xử lý xóa vĩnh viễn khỏi Database
        // ==========================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUser = User.Identity?.Name;
            var userToDelete = await _context.Users.FindAsync(id);

            if (userToDelete == null) return NotFound();

            // Lớp bảo vệ kép: Kiểm tra lại một lần nữa ở Backend
            if (userToDelete.UserName == currentUser)
            {
                TempData["Error"] = "Hệ thống từ chối: Bạn không thể tự xóa tài khoản của chính mình!";
                return RedirectToAction(nameof(Index));
            }

            _context.Users.Remove(userToDelete);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã xóa tài khoản '{userToDelete.UserName}' thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}