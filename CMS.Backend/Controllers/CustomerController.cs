using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using CMS.Data;
using CMS.Data.Entities;

namespace CMS.Backend.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")] // Khách hàng là dữ liệu nhạy cảm
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<Customer> _hasher;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
            _hasher = new PasswordHasher<Customer>();
        }

        // ==========================================
        // 1. GET: Danh sách khách hàng
        // ==========================================
        public async Task<IActionResult> Index()
        {
            // Sắp xếp khách hàng mới nhất lên đầu
            var customers = await _context.Customers.OrderByDescending(c => c.CreatedDate).ToListAsync();
            return View(customers);
        }

        // ==========================================
        // 2. GET: Form thêm khách hàng
        // ==========================================
        public IActionResult Create() => View();

        // ==========================================
        // 3. POST: Xử lý thêm khách hàng
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer, string RawPassword)
        {
            // Loại bỏ kiểm tra PasswordHash vì ta sẽ tự băm
            ModelState.Remove("PasswordHash");

            if (string.IsNullOrWhiteSpace(RawPassword))
                ModelState.AddModelError("RawPassword", "Vui lòng nhập mật khẩu cho khách hàng!");

            // Kiểm tra trùng Email
            if (await _context.Customers.AnyAsync(c => c.Email == customer.Email))
                ModelState.AddModelError("Email", "Địa chỉ Email này đã có người đăng ký!");

            if (ModelState.IsValid)
            {
                customer.PasswordHash = _hasher.HashPassword(customer, RawPassword);
                customer.CreatedDate = DateTime.Now;

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm khách hàng mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // ==========================================
        // 4. GET: Form sửa khách hàng
        // ==========================================
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        // ==========================================
        // 5. POST: Xử lý cập nhật khách hàng
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Customer model, string? NewPassword)
        {
            var existingCustomer = await _context.Customers.FindAsync(model.Id);
            if (existingCustomer == null) return NotFound();

            ModelState.Remove("PasswordHash");

            // Kiểm tra trùng Email (Trừ chính khách hàng này)
            if (await _context.Customers.AnyAsync(c => c.Email == model.Email && c.Id != model.Id))
                ModelState.AddModelError("Email", "Email này đã bị khách hàng khác sử dụng!");

            if (ModelState.IsValid)
            {
                existingCustomer.FullName = model.FullName;
                existingCustomer.Email = model.Email;
                existingCustomer.Phone = model.Phone;
                existingCustomer.Address = model.Address;
                existingCustomer.IsActive = model.IsActive;

                // Cập nhật mật khẩu nếu Admin có gõ mật khẩu mới
                if (!string.IsNullOrWhiteSpace(NewPassword))
                {
                    existingCustomer.PasswordHash = _hasher.HashPassword(existingCustomer, NewPassword);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật thông tin khách hàng thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // ==========================================
        // 6. GET & POST: Xóa khách hàng
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã xóa khách hàng '{customer.FullName}' thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}