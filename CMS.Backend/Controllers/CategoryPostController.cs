using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;
using System.Text;
using CMS.Data;
using CMS.Data.Entities;

namespace CMS.Controllers // Hoặc CMS.Backend.Controllers tùy cấu trúc thư mục của bạn
{
    [Authorize(Roles = "SuperAdmin,Admin,Editor")] // 1. BẢO MẬT: Cho phép SuperAdmin, Admin và Editor
    public class CategoryPostController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryPostController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. DANH SÁCH DANH MỤC TIN TỨC
        // ==========================================
        public async Task<IActionResult> Index()
        {
            // Tối ưu hiệu năng bằng Async và Include để lấy tên Danh mục Cha
            var categories = await _context.CategoryPosts
                .Include(c => c.ParentCategory)
                .AsNoTracking()
                .OrderBy(c => c.ParentId) // Đưa danh mục cha lên đầu
                .ThenBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }

        // ==========================================
        // 2. FORM TẠO MỚI (GET)
        // ==========================================
        [HttpGet]
        public IActionResult Create()
        {
            PrepareParentDropdown();
            return View();
        }

        // ==========================================
        // 3. XỬ LÝ LƯU (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryPost model)
        {
            if (ModelState.IsValid)
            {
                // Tự động tạo link SEO (Slug) nếu Admin quên nhập
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    model.Slug = GenerateSlug(model.Name);
                }

                _context.CategoryPosts.Add(model);
                await _context.SaveChangesAsync(); // Đã chuyển sang Async
                return RedirectToAction(nameof(Index));
            }

            PrepareParentDropdown(model.ParentId);
            return View(model);
        }

        // ==========================================
        // 4. FORM SỬA (GET)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.CategoryPosts.FindAsync(id);
            if (category == null) return NotFound();

            PrepareParentDropdown(category.ParentId, category.Id);
            return View(category);
        }

        // ==========================================
        // 5. XỬ LÝ SỬA (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryPost model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                // Ngăn chặn việc gán danh mục cha là chính nó (Gây lỗi vòng lặp)
                if (model.ParentId == model.Id)
                {
                    ModelState.AddModelError("ParentId", "Danh mục không thể làm con của chính nó.");
                    PrepareParentDropdown(model.ParentId, model.Id);
                    return View(model);
                }

                // Tự động cập nhật lại Slug nếu Admin để trống
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    model.Slug = GenerateSlug(model.Name);
                }

                _context.CategoryPosts.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            PrepareParentDropdown(model.ParentId, model.Id);
            return View(model);
        }

        // ==========================================
        // 6. XÓA DANH MỤC
        // ==========================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.CategoryPosts.FindAsync(id);
            if (category != null)
            {
                // KIỂM TRA BẢO MẬT: Có danh mục con hoặc bài viết nào đang phụ thuộc không?
                bool hasChildren = await _context.CategoryPosts.AnyAsync(c => c.ParentId == id);
                bool hasPosts = await _context.Posts.AnyAsync(p => p.CategoryPostId == id);

                if (hasChildren || hasPosts)
                {
                    TempData["ErrorMessage"] = "Không thể xóa! Vẫn còn danh mục con hoặc bài viết đang thuộc danh mục này.";
                    return RedirectToAction(nameof(Index));
                }

                _context.CategoryPosts.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // CÁC HÀM HỖ TRỢ NỘI BỘ (HELPERS)
        // ==========================================

        // Hàm đổ dữ liệu Dropdown cho cấu trúc Cha - Con
        private void PrepareParentDropdown(int? selectedId = null, int? currentId = null)
        {
            var query = _context.CategoryPosts.Where(c => c.ParentId == null);

            // Nếu đang Edit, loại bỏ chính nó khỏi danh sách Cha
            if (currentId.HasValue)
            {
                query = query.Where(c => c.Id != currentId.Value);
            }

            ViewBag.ParentId = new SelectList(query, "Id", "Name", selectedId);
        }

        // Hàm tự động tạo Slug chuẩn SEO từ Tên danh mục (VD: "Tin Thể Thao" -> "tin-the-thao")
        private string GenerateSlug(string phrase)
        {
            string str = phrase.ToLower().Trim();
            // Xóa dấu tiếng Việt
            str = Regex.Replace(str, @"[áàảãạăắằẳẵặâấầẩẫậ]", "a");
            str = Regex.Replace(str, @"[éèẻẽẹêếềểễệ]", "e");
            str = Regex.Replace(str, @"[íìỉĩị]", "i");
            str = Regex.Replace(str, @"[óòỏõọôốồổỗộơớờởỡợ]", "o");
            str = Regex.Replace(str, @"[úùủũụưứừửữự]", "u");
            str = Regex.Replace(str, @"[ýỳỷỹỵ]", "y");
            str = Regex.Replace(str, @"[đ]", "d");

            // Xóa các ký tự đặc biệt, thay khoảng trắng bằng dấu gạch ngang
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = Regex.Replace(str, @"\s", "-");
            return str;
        }
    }
}