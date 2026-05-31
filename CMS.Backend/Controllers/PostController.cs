using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CMS.Controllers
{
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH
        public async Task<IActionResult> Index()
        {
            return View(await _context.Posts.AsNoTracking().Include(p => p.Category).OrderByDescending(p => p.CreatedDate).ToListAsync());
        }

        // 2. CHI TIẾT
        public async Task<IActionResult> Details(int id)
        {
            var post = await _context.Posts.AsNoTracking().Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
            return post == null ? NotFound() : View(post);
        }

        // 3. THÊM MỚI
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Post model, IFormFile? uploadImage)
        {
            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                if (uploadImage != null && uploadImage.Length > 0)
                    model.ImageUrl = await SaveFileAsync(uploadImage);

                _context.Posts.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);
            return View(model);
        }

        // 4. CHỈNH SỬA
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _context.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (post == null) return NotFound();

            ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Name", post.CategoryId);
            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Post model, IFormFile? uploadImage)
        {
            ModelState.Remove("ImageUrl");

            if (!ModelState.IsValid)
            {
                ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);
                return View(model);
            }

            // Lấy ra bài viết hiện tại trong database
            var existingPost = await _context.Posts.FirstOrDefaultAsync(p => p.Id == model.Id);
            if (existingPost == null) return NotFound();

            // Cập nhật các trường dữ liệu
            existingPost.Title = model.Title;
            existingPost.Content = model.Content;
            existingPost.CategoryId = model.CategoryId;

            // Xử lý ảnh
            if (uploadImage != null && uploadImage.Length > 0)
            {
                DeleteFile(existingPost.ImageUrl);
                existingPost.ImageUrl = await SaveFileAsync(uploadImage);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Không thể lưu thay đổi vào cơ sở dữ liệu.");
                ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // 5. XÓA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                DeleteFile(post.ImageUrl);
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // --- HÀM HỖ TRỢ ---
        private async Task<string> SaveFileAsync(IFormFile file)
        {
            string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            using (var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return "/uploads/" + fileName;
        }

        private void DeleteFile(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imageUrl.TrimStart('/'));
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        }
    }
}