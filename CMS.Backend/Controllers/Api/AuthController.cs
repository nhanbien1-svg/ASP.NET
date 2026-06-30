using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CMS.Data;
using CMS.Data.Entities;

namespace CMS.Backend.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        // 1. Cập nhật Hasher sang kiểu Customer
        private readonly PasswordHasher<Customer> _hasher;
        private readonly IConfiguration _config;

        public AuthController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _hasher = new PasswordHasher<Customer>();
            _config = config;
        }

        // ==========================================
        // 1. API ĐĂNG KÝ (LƯU VÀO BẢNG CUSTOMERS)
        // Endpoint: POST /api/auth/register
        // ==========================================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Kiểm tra trùng Email trong bảng Khách hàng
            if (await _context.Customers.AnyAsync(c => c.Email == request.Email))
            {
                return BadRequest(new { message = "Email này đã được sử dụng!" });
            }

            var newCustomer = new Customer
            {
                FullName = request.FullName,
                Email = request.Email,
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            // Băm mật khẩu và lưu vào PasswordHash
            newCustomer.PasswordHash = _hasher.HashPassword(newCustomer, request.Password);

            _context.Customers.Add(newCustomer);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công!" });
        }

        // ==========================================
        // 2. API ĐĂNG NHẬP (KIỂM TRA BẢNG CUSTOMERS)
        // Endpoint: POST /api/auth/login
        // ==========================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Truy vấn từ bảng Khách hàng
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == request.Email);

            if (customer == null || !customer.IsActive)
            {
                return Unauthorized(new { message = "Tài khoản không tồn tại hoặc đã bị khóa." });
            }

            // Kiểm tra mật khẩu băm
            var result = _hasher.VerifyHashedPassword(customer, customer.PasswordHash, request.Password);
            if (result != PasswordVerificationResult.Success)
            {
                return Unauthorized(new { message = "Sai mật khẩu." });
            }

            // Tạo Token (Căn cước điện tử)
            var token = GenerateJwtToken(customer);

            return Ok(new
            {
                message = "Đăng nhập thành công!",
                token = token,
                user = new
                {
                    FullName = customer.FullName,
                    Email = customer.Email,
                    Role = "Customer" // Trả về role cứng để React nhận diện
                }
            });
        }

        // ==========================================
        // Hàm hỗ trợ tạo Token cho Customer
        // ==========================================
        private string GenerateJwtToken(Customer customer)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                // Customer không có UserName, dùng Id hoặc Email làm định danh chính (Sub)
                new Claim(JwtRegisteredClaimNames.Sub, customer.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, customer.Email),
                new Claim("FullName", customer.FullName),
                new Claim(ClaimTypes.Role, "Customer")
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    // Các class hỗ trợ nhận dữ liệu từ Frontend
    public class RegisterRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}