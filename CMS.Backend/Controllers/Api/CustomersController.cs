using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization; // Cần thiết để dùng [AllowAnonymous]
using CMS.Data;
using CMS.Data.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

using CMS.Backend.Services; // Thêm namespace này

namespace CMS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous] // Cho phép khách hàng truy cập API này mà không cần quyền Admin
    public class CustomersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<Customer> _hasher;
        private readonly IEmailService _emailService;

        public CustomersController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
            _hasher = new PasswordHasher<Customer>();
        }

        // ==========================================
        // 1. API ĐĂNG KÝ
        // ==========================================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CustomerRegisterDto model)
        {
            // Kiểm tra null và dữ liệu rỗng
            if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
            {
                return BadRequest(new { message = "Thông tin đăng ký không hợp lệ!" });
            }

            try
            {
                // Kiểm tra email tồn tại (sử dụng ToLower() và Trim() để so sánh chuẩn xác)
                var emailNormalized = model.Email.Trim().ToLower();
                var isEmailExist = await _context.Customers
                    .AnyAsync(c => c.Email.ToLower() == emailNormalized);

                if (isEmailExist)
                {
                    return BadRequest(new { message = "Email này đã được đăng ký!" });
                }

                var newCustomer = new Customer
                {
                    FullName = model.FullName?.Trim() ?? "Khách hàng mới",
                    Email = emailNormalized,
                    Phone = model.Phone?.Trim(),
                    Address = model.Address?.Trim(),
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                // Băm mật khẩu trước khi lưu
                newCustomer.PasswordHash = _hasher.HashPassword(newCustomer, model.Password);

                _context.Customers.Add(newCustomer);
                await _context.SaveChangesAsync();

                return StatusCode(201, new { message = "Tạo tài khoản thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi máy chủ: " + ex.Message });
            }
        }

        // ==========================================
        // 2. API ĐĂNG NHẬP
        // ==========================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] CustomerLoginDto model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Email))
            {
                return BadRequest(new { message = "Vui lòng nhập email." });
            }

            try
            {
                var emailNormalized = model.Email.Trim().ToLower();

                // Tìm kiếm khách hàng bằng AsNoTracking() để truy vấn nhanh hơn
                var customer = await _context.Customers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Email.ToLower() == emailNormalized);

                if (customer == null)
                {
                    return Unauthorized(new { message = "Email hoặc mật khẩu không chính xác!" });
                }

                // Verify mật khẩu đã băm
                var result = _hasher.VerifyHashedPassword(customer, customer.PasswordHash, model.Password);

                if (result == PasswordVerificationResult.Failed)
                {
                    return Unauthorized(new { message = "Email hoặc mật khẩu không chính xác!" });
                }

                // Trả về dữ liệu phiên làm việc
                return Ok(new
                {
                    id = customer.Id,
                    fullName = customer.FullName,
                    email = customer.Email,
                    phone = customer.Phone,
                    address = customer.Address,
                    avatarUrl = customer.AvatarUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi máy chủ: " + ex.Message });
            }
        } // Bổ sung dấu ngoặc đóng cho hàm Login

        // ==========================================
        // 3. API CẬP NHẬT HỒ SƠ
        // ==========================================
        [HttpPut("{id}/profile")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] CustomerUpdateProfileDto model)
        {
            if (model == null)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ." });
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản khách hàng." });
            }

            try
            {
                customer.FullName = model.FullName?.Trim() ?? customer.FullName;
                customer.Phone = model.Phone?.Trim();
                customer.Address = model.Address?.Trim();
                customer.AvatarUrl = model.AvatarUrl?.Trim();

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Cập nhật hồ sơ thành công",
                    customer = new
                    {
                        id = customer.Id,
                        fullName = customer.FullName,
                        email = customer.Email,
                        phone = customer.Phone,
                        address = customer.Address,
                        avatarUrl = customer.AvatarUrl
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi máy chủ: " + ex.Message });
            }
        }

        // ==========================================
        // 4. API TẢI LÊN ẢNH ĐẠI DIỆN TỪ MÁY TÍNH
        // ==========================================
        [HttpPost("{id}/avatar")]
        public async Task<IActionResult> UploadAvatar(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Không có file nào được chọn." });
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản khách hàng." });
            }

            try
            {
                // Tạo thư mục nếu chưa có
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatars");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Tạo tên file độc nhất tránh trùng lặp
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Lưu đường dẫn tương đối vào database
                var fileUrl = $"/images/avatars/{uniqueFileName}";
                customer.AvatarUrl = fileUrl;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Tải ảnh đại diện thành công!",
                    avatarUrl = fileUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi máy chủ: " + ex.Message });
            }
        }

        // ==========================================
        // 5. API QUÊN MẬT KHẨU
        // ==========================================
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] CustomerForgotPasswordDto model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Email))
            {
                return BadRequest(new { message = "Vui lòng nhập email." });
            }

            var emailNormalized = model.Email.Trim().ToLower();
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email.ToLower() == emailNormalized);

            if (customer == null)
            {
                return NotFound(new { message = "Email này chưa được đăng ký trong hệ thống!" });
            }

            try
            {
                // Tạo mật khẩu ngẫu nhiên 6 số
                var random = new Random();
                var tempPassword = random.Next(100000, 999999).ToString();

                // Băm mật khẩu mới
                customer.PasswordHash = _hasher.HashPassword(customer, tempPassword);
                await _context.SaveChangesAsync();

                // Gửi Email
                var subject = "Yêu cầu cấp lại mật khẩu - TechZone";
                var body = $"Chào {customer.FullName},<br/><br/>Bạn vừa yêu cầu cấp lại mật khẩu. Mật khẩu tạm thời của bạn là: <b>{tempPassword}</b><br/>Vui lòng đăng nhập và đổi mật khẩu ngay lập tức.<br/><br/>Trân trọng,<br/>Đội ngũ TechZone.";

                await _emailService.SendEmailAsync(customer.Email, subject, body);

                return Ok(new { message = "Mật khẩu mới đã được gửi đến Email của bạn!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi máy chủ: " + ex.Message });
            }
        }
    }

    // DTOs
    public class CustomerRegisterDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string Password { get; set; }
    }

    public class CustomerLoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class CustomerUpdateProfileDto
    {
        public string FullName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class CustomerForgotPasswordDto
    {
        public string Email { get; set; }
    }
}