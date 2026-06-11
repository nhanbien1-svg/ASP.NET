using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using CMS.Data.Entities;

namespace CMS.Backend.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ==================================================
        // 1. CÁC PHƯƠNG THỨC GIAO DIỆN (ADMIN - MVC)
        // ==================================================

        public async Task<IActionResult> Index()
        {
            return View(await _context.Products.Include(p => p.CategoryProduct).ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["CategoryProductId"] = new SelectList(_context.CategoryProducts, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                product.ImageUrl = await UploadImage(imageFile);
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryProductId"] = new SelectList(_context.CategoryProducts, "Id", "Name", product.CategoryProductId);
            return View(product);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            ViewData["CategoryProductId"] = new SelectList(_context.CategoryProducts, "Id", "Name", product.CategoryProductId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile)
        {
            if (id != product.Id) return NotFound();
            if (ModelState.IsValid)
            {
                if (imageFile != null) product.ImageUrl = await UploadImage(imageFile);
                _context.Update(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var product = await _context.Products.FindAsync(id);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Hàm hỗ trợ upload ảnh (Tái sử dụng)
        private async Task<string> UploadImage(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string path = Path.Combine(_webHostEnvironment.WebRootPath, "images/products", fileName);
            using (var stream = new FileStream(path, FileMode.Create)) { await file.CopyToAsync(stream); }
            return "/images/products/" + fileName;
        }

        // ==================================================
        // 2. CÁC PHƯƠNG THỨC API (CHO FRONTEND REACT)
        // ==================================================

        [HttpGet("api/products")]
        public async Task<IActionResult> GetProductsApi()
        {
            return Ok(await _context.Products.Select(p => new {
                p.Id,
                p.Name,
                p.Price,
                p.ImageUrl,
                CategoryName = p.CategoryProduct != null ? p.CategoryProduct.Name : "Chưa phân loại"
            }).ToListAsync());
        }

        [HttpGet("api/products/{id}")]
        public async Task<IActionResult> GetProductDetailsApi(int id)
        {
            var product = await _context.Products.Where(p => p.Id == id).Select(p => new {
                p.Id,
                p.Name,
                p.Price,
                p.ImageUrl,
                p.StockQuantity,
                CategoryName = p.CategoryProduct != null ? p.CategoryProduct.Name : "Chưa phân loại"
            }).FirstOrDefaultAsync();
            return product == null ? NotFound() : Ok(product);
        }
    }
}