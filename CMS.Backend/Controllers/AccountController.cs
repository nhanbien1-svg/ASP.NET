using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CMS.Data;
using CMS.Data.Entities;

namespace CMS.Backend.Controllers
{
    [AllowAnonymous] // BẮT BUỘC: Cho phép khách truy cập trang đăng nhập và chạy file cài đặt
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        // ==========================================
        // 1. GET: HIỂN THỊ TRANG ĐĂNG NHẬP
        // ==========================================
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Đã đăng nhập rồi thì đẩy về Home, không cho ở lại trang Login
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // ==========================================
        // 2. POST: XỬ LÝ ĐĂNG NHẬP
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            // Chặn rỗng ngay từ Controller
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.";
                return View();
            }

            // Tìm kiếm user (Dùng Async tối ưu hiệu năng)
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);

            if (user != null)
            {
                // Kiểm tra trạng thái khóa tài khoản
                if (!user.IsActive)
                {
                    ViewBag.Error = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.";
                    return View();
                }

                // Xác thực mật khẩu
                var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

                if (result == PasswordVerificationResult.Success)
                {
                    // Đúc "Thẻ căn cước" (Claims)
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Role, user.Role ?? "Admin"),
                        new Claim("FullName", user.FullName ?? user.UserName)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    // Cấu hình phiên đăng nhập (Session/Cookie)
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) // Lưu đăng nhập 7 ngày
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // Chống lỗi Open Redirect
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }
            }

            // Báo lỗi chung để bảo mật
            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không chính xác.";
            return View();
        }

        // ==========================================
        // 3. ĐĂNG XUẤT
        // ==========================================
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        // ==========================================
        // 4. TRANG BÁO LỖI KHÔNG ĐỦ QUYỀN (403)
        // ==========================================
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // ==========================================
        // 5. TOOL KHỞI TẠO ADMIN ĐẦU TIÊN (MỚI THÊM)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> SetupFirstAdmin()
        {
            // Kiểm tra: Nếu DB đã có người, lập tức khóa tool này lại
            if (await _context.Users.AnyAsync())
            {
                return Content("Hệ thống đã có dữ liệu tài khoản. Chức năng khởi tạo này đã tự động bị khóa để đảm bảo an toàn!");
            }

            // Tạo tài khoản mặc định
            var superAdmin = new User
            {
                UserName = "superadmin",
                FullName = "Quản trị viên cấp cao",
                Email = "admin@thaicms.com",
                Role = "SuperAdmin",
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            // Băm mật khẩu (Pass mặc định: 123456)
            var hasher = new PasswordHasher<User>();
            superAdmin.PasswordHash = hasher.HashPassword(superAdmin, "123456");

            _context.Users.Add(superAdmin);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã khởi tạo tài khoản Super Admin thành công! Vui lòng dùng tài khoản 'superadmin' và mật khẩu '123456' để đăng nhập.";
            return RedirectToAction("Login");
        }
    }
}