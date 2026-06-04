using CMS.Data;
using CMS.Data.Entities;
using CMS.DATA.Entities;
using Microsoft.AspNetCore.Authorization; // Thư viện bảo mật
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CMS.Controllers
{
    [Authorize] // 1. BẮT BUỘC ĐĂNG NHẬP MỚI VÀO ĐƯỢC CÁC TRANG TRONG CONTROLLER NÀY
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH DANH MỤC
        public IActionResult Index()
        {
            var data = _context.Categories.ToList();
            return View(data);
        }

        // 2. FORM TẠO MỚI (GET)
        [HttpGet]
        public IActionResult Create() => View();

        // 3. XỬ LÝ LƯU (POST)
        [HttpPost]
        [ValidateAntiForgeryToken] // Chống tấn công giả mạo (CSRF)
        public IActionResult Create(Category model)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(model);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // 4. FORM SỬA (GET)
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // 5. XỬ LÝ SỬA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category model)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Update(model);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // 6. XÓA DANH MỤC (CHỈ ADMIN MỚI CÓ QUYỀN)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // 2. PHÂN QUYỀN: Chỉ Admin mới thực hiện được
        public IActionResult DeleteConfirmed(int id)
        {
            var category = _context.Categories.Find(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}