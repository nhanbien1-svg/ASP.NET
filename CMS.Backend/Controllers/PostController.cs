using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.RegularExpressions;
using CMS.Data;
using CMS.Data.Entities;

namespace CMS.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,Editor")] // Đã sửa: Cho phép cả 3 role truy cập
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PostController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ==========================================
        // 1. DANH SÁCH BÀI VIẾT
        // ==========================================
        public async Task<IActionResult> Index()
        {
            // ĐÃ SỬA: Include CategoryPost thay vì Category
            var posts = await _context.Posts
                .AsNoTracking()
                .Include(p => p.CategoryPost)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
            return View(posts);
        }

        // ==========================================
        // 2. CHI TIẾT BÀI VIẾT (Dành cho Admin xem trước)
        // ==========================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var post = await _context.Posts
                .AsNoTracking()
                .Include(p => p.CategoryPost)
                .FirstOrDefaultAsync(p => p.Id == id);

            return post == null ? NotFound() : View(post);
        }

        // ==========================================
        // 3. THÊM MỚI BÀI VIẾT
        // ==========================================
        [HttpGet]
        public IActionResult Create()
        {
            PrepareCategoryDropdown(); // Dùng hàm tự viết để hiển thị Dropdown đẹp hơn
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Post model, IFormFile? uploadImage)
        {
            ModelState.Remove("ImageUrl"); // Bỏ qua validate vì tự xử lý logic file
            ModelState.Remove("Slug");     // Bỏ qua validate vì hệ thống sẽ tự tạo

            if (ModelState.IsValid)
            {
                // Tự động tạo Link chuẩn SEO từ Tiêu đề
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    model.Slug = GenerateSlug(model.Title);
                }

                if (uploadImage != null)
                    model.ImageUrl = await SaveFileAsync(uploadImage);

                model.CreatedDate = DateTime.Now;
                _context.Posts.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            PrepareCategoryDropdown(model.CategoryPostId);
            return View(model);
        }

        // ==========================================
        // 4. CHỈNH SỬA BÀI VIẾT
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            PrepareCategoryDropdown(post.CategoryPostId);
            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Post model, IFormFile? uploadImage)
        {
            ModelState.Remove("ImageUrl");
            ModelState.Remove("Slug");

            if (!ModelState.IsValid)
            {
                PrepareCategoryDropdown(model.CategoryPostId);
                return View(model);
            }

            var existingPost = await _context.Posts.FindAsync(model.Id);
            if (existingPost == null) return NotFound();

            // Cập nhật thông tin cơ bản & SEO
            existingPost.Title = model.Title;
            existingPost.Summary = model.Summary;
            existingPost.Content = model.Content;
            existingPost.IsPublished = model.IsPublished;
            existingPost.CategoryPostId = model.CategoryPostId;

            // Cập nhật Slug nếu rỗng
            existingPost.Slug = string.IsNullOrWhiteSpace(model.Slug) ? GenerateSlug(model.Title) : model.Slug;

            // Xử lý ảnh mới đè ảnh cũ
            if (uploadImage != null)
            {
                DeleteFile(existingPost.ImageUrl);
                existingPost.ImageUrl = await SaveFileAsync(uploadImage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 5. XÓA BÀI VIẾT (Phân quyền Admin)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                DeleteFile(post.ImageUrl); // Xóa luôn ảnh trên server cho nhẹ ổ cứng
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // CÁC HÀM HỖ TRỢ (HELPERS)
        // ==========================================

        // Hiển thị danh mục theo dạng: "Danh mục cha > Danh mục con"
        private void PrepareCategoryDropdown(int? selectedId = null)
        {
            var categories = _context.CategoryPosts
                .Include(c => c.ParentCategory)
                .Select(c => new {
                    c.Id,
                    FullName = c.ParentId != null ? c.ParentCategory.Name + " > " + c.Name : c.Name
                })
                .OrderBy(c => c.FullName)
                .ToList();

            ViewBag.CategoryList = new SelectList(categories, "Id", "FullName", selectedId);
        }

        // Tự động tạo link tĩnh SEO từ Tiêu đề
        private string GenerateSlug(string phrase)
        {
            string str = phrase.ToLower().Trim();
            str = Regex.Replace(str, @"[áàảãạăắằẳẵặâấầẩẫậ]", "a");
            str = Regex.Replace(str, @"[éèẻẽẹêếềểễệ]", "e");
            str = Regex.Replace(str, @"[íìỉĩị]", "i");
            str = Regex.Replace(str, @"[óòỏõọôốồổỗộơớờởỡợ]", "o");
            str = Regex.Replace(str, @"[úùủũụưứừửữự]", "u");
            str = Regex.Replace(str, @"[ýỳỷỹỵ]", "y");
            str = Regex.Replace(str, @"[đ]", "d");
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = str.Substring(0, str.Length <= 100 ? str.Length : 100).Trim(); // Link bài viết có thể dài hơn link danh mục
            str = Regex.Replace(str, @"\s", "-");
            return str;
        }

        // --- CÁC HÀM HỖ TRỢ XỬ LÝ FILE (Đã đổi thư mục sang images/posts cho chuẩn) ---
        private async Task<string> SaveFileAsync(IFormFile file)
        {
            string uploadsFolder = Path.Combine(_env.WebRootPath, "images", "posts");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return "/images/posts/" + uniqueFileName;
        }

        private void DeleteFile(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;
            string path = Path.Combine(_env.WebRootPath, imageUrl.TrimStart('/'));
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        }
    }
}