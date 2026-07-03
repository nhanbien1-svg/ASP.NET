using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using CMS.Data;
using CMS.Data.Entities;

namespace CMS.Backend.Controllers
{
    // Yêu cầu quyền Admin để truy cập khu vực này
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class ProductCategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env; // Biến môi trường để xử lý lưu file ảnh

        public ProductCategoryController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ==========================================
        // 1. DANH SÁCH DANH MỤC
        // ==========================================
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách, gộp thêm thông tin danh mục Cha để hiển thị
            var categories = await _context.CategoryProducts
                .Include(c => c.ParentCategory)
                .AsNoTracking()
                .OrderBy(c => c.ParentId) // Sắp xếp danh mục Cha lên trước
                .ThenBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }

        // ==========================================
        // 2. THÊM MỚI DANH MỤC (GET)
        // ==========================================
        public IActionResult Create()
        {
            // Đẩy danh sách các Danh mục gốc (ParentId == null) sang View để làm thẻ <select>
            ViewBag.ParentCategories = new SelectList(_context.CategoryProducts.Where(c => c.ParentId == null), "Id", "Name");
            return View();
        }

        // ==========================================
        // 3. THÊM MỚI DANH MỤC (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryProduct categoryProduct, IFormFile? uploadImage)
        {
            ModelState.Remove("ImageUrl"); // Bỏ qua validate bắt buộc đối với cột ảnh

            if (ModelState.IsValid)
            {
                // Xử lý upload ảnh Logo/Icon nếu có
                if (uploadImage != null)
                {
                    categoryProduct.ImageUrl = await SaveFileAsync(uploadImage);
                }

                _context.Add(categoryProduct);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ParentCategories = new SelectList(_context.CategoryProducts.Where(c => c.ParentId == null), "Id", "Name", categoryProduct.ParentId);
            return View(categoryProduct);
        }

        // ==========================================
        // 4. CHỈNH SỬA DANH MỤC (GET)
        // ==========================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.CategoryProducts.FindAsync(id);
            if (category == null) return NotFound();

            // Đẩy danh sách Danh mục gốc, nhưng loại trừ chính nó để tránh tự làm cha của mình
            ViewBag.ParentCategories = new SelectList(_context.CategoryProducts.Where(c => c.ParentId == null && c.Id != id), "Id", "Name", category.ParentId);
            return View(category);
        }

        // ==========================================
        // 5. CHỈNH SỬA DANH MỤC (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryProduct categoryProduct, IFormFile? uploadImage)
        {
            if (id != categoryProduct.Id) return NotFound();

            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                // Ngăn chặn việc gán danh mục cha là chính nó
                if (categoryProduct.ParentId == categoryProduct.Id)
                {
                    ModelState.AddModelError("ParentId", "Danh mục không thể làm con của chính nó.");
                    ViewBag.ParentCategories = new SelectList(_context.CategoryProducts.Where(c => c.ParentId == null && c.Id != id), "Id", "Name", categoryProduct.ParentId);
                    return View(categoryProduct);
                }

                var existingCategory = await _context.CategoryProducts.FindAsync(id);
                if (existingCategory == null) return NotFound();

                // Cập nhật thông tin text
                existingCategory.Name = categoryProduct.Name;
                existingCategory.Description = categoryProduct.Description;
                existingCategory.ParentId = categoryProduct.ParentId;

                // Cập nhật ảnh: Xóa ảnh cũ đi và lưu ảnh mới
                if (uploadImage != null)
                {
                    DeleteFile(existingCategory.ImageUrl);
                    existingCategory.ImageUrl = await SaveFileAsync(uploadImage);
                }

                _context.Update(existingCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ParentCategories = new SelectList(_context.CategoryProducts.Where(c => c.ParentId == null && c.Id != id), "Id", "Name", categoryProduct.ParentId);
            return View(categoryProduct);
        }

        // ==========================================
        // 6. XÓA DANH MỤC (GET)
        // ==========================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.CategoryProducts
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);

            return category == null ? NotFound() : View(category);
        }

        // ==========================================
        // 7. XÓA DANH MỤC (POST)
        // ==========================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.CategoryProducts.FindAsync(id);
            if (category != null)
            {
                // KIỂM TRA BẢO MẬT: Có danh mục con nào đang phụ thuộc không?
                bool hasChildren = await _context.CategoryProducts.AnyAsync(c => c.ParentId == id);
                if (hasChildren)
                {
                    ModelState.AddModelError("", "Lỗi: Không thể xóa danh mục này vì vẫn còn các danh mục con phụ thuộc bên trong.");
                    return View(category);
                }

                // Xóa file ảnh vật lý trong thư mục wwwroot
                DeleteFile(category.ImageUrl);

                // Xóa record trong database
                _context.CategoryProducts.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // CÁC HÀM HỖ TRỢ XỬ LÝ FILE ẢNH
        // ==========================================
        private async Task<string> SaveFileAsync(IFormFile file)
        {
            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "categories");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return "/uploads/categories/" + uniqueFileName;
        }

        private void DeleteFile(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;
            string path = Path.Combine(_env.WebRootPath, imageUrl.TrimStart('/'));
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        }
    }
}