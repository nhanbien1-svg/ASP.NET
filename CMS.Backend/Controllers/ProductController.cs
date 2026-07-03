using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using CMS.Data;
using CMS.Data.Entities;

namespace CMS.Backend.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")] // Cấm Editor truy cập Products
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
            // Lấy danh sách sản phẩm, sắp xếp sản phẩm mới nhất lên đầu
            var products = await _context.Products
                .Include(p => p.CategoryProduct)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
            return View(products);
        }

        public IActionResult Create()
        {
            PrepareCategoryDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            ModelState.Remove("ImageUrl"); // Bỏ qua kiểm tra lỗi cột ảnh

            if (ModelState.IsValid)
            {
                product.ImageUrl = await UploadImage(imageFile);
                product.CreatedDate = DateTime.Now; // Tự động gán ngày tạo

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PrepareCategoryDropdown(product.CategoryProductId);
            return View(product);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            PrepareCategoryDropdown(product.CategoryProductId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile)
        {
            if (id != product.Id) return NotFound();
            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                var existingProduct = await _context.Products.FindAsync(id);
                if (existingProduct == null) return NotFound();

                // Cập nhật thông tin text
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.StockQuantity = product.StockQuantity;
                existingProduct.IsActive = product.IsActive;
                existingProduct.CategoryProductId = product.CategoryProductId;

                // Cập nhật ảnh nếu có file mới
                if (imageFile != null)
                {
                    DeleteOldImage(existingProduct.ImageUrl);
                    existingProduct.ImageUrl = await UploadImage(imageFile);
                }

                _context.Update(existingProduct);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PrepareCategoryDropdown(product.CategoryProductId);
            return View(product);
        }

        // Thay vì xóa cứng (Hard Delete), chúng ta chuyển trạng thái IsActive = false
        public async Task<IActionResult> Delete(int? id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.IsActive = false; // Xóa mềm (Soft Delete)
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã ẩn sản phẩm thành công.";
            }
            return RedirectToAction(nameof(Index));
        }

        // Xóa cứng (Hard Delete) - Xóa vĩnh viễn khỏi Database
        public async Task<IActionResult> HardDelete(int? id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // Kiểm tra xem sản phẩm đã có trong đơn hàng nào chưa
                bool hasOrders = await _context.OrderDetails.AnyAsync(od => od.ProductId == id);
                if (hasOrders)
                {
                    TempData["Error"] = "Không thể xóa sản phẩm này vì đã phát sinh đơn hàng. Vui lòng sử dụng tính năng 'Ngừng kinh doanh' (Ẩn sản phẩm) để bảo toàn dữ liệu thống kê.";
                    return RedirectToAction(nameof(Index));
                }

                // Xóa ảnh cũ
                DeleteOldImage(product.ImageUrl);

                // Xóa các dữ liệu liên quan khác nếu cấu hình DB chưa cascade (Review, CartItem)
                var reviews = _context.Reviews.Where(r => r.ProductId == id);
                _context.Reviews.RemoveRange(reviews);

                var cartItems = _context.CartItems.Where(c => c.ProductId == id);
                _context.CartItems.RemoveRange(cartItems);

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa vĩnh viễn sản phẩm thành công.";
            }
            return RedirectToAction(nameof(Index));
        }

        // ==================================================
        // CÁC HÀM HỖ TRỢ XỬ LÝ NỘI BỘ DÀNH CHO ADMIN
        // ==================================================

        // Hiển thị danh mục theo dạng: "Danh mục cha > Danh mục con" để Admin dễ chọn
        private void PrepareCategoryDropdown(int? selectedId = null)
        {
            var categories = _context.CategoryProducts
                .Include(c => c.ParentCategory)
                .Select(c => new {
                    c.Id,
                    FullName = c.ParentId != null ? c.ParentCategory.Name + " > " + c.Name : c.Name
                })
                .OrderBy(c => c.FullName)
                .ToList();

            ViewData["CategoryProductId"] = new SelectList(categories, "Id", "FullName", selectedId);
        }

        private async Task<string> UploadImage(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath); // Đảm bảo thư mục tồn tại

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create)) { await file.CopyToAsync(stream); }
            return "/images/products/" + fileName;
        }

        private void DeleteOldImage(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;
            string path = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'));
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        }

        // ==================================================
        // 2. CÁC PHƯƠNG THỨC API (CHO FRONTEND REACT)
        // ==================================================

        /// <summary>
        /// Lấy danh sách sản phẩm với bộ lọc nâng cao
        /// </summary>
        /// <param name="category">Tên danh mục cần lọc (VD: Laptop, PC)</param>
        /// <param name="search">Từ khóa tìm kiếm theo tên hoặc mô tả</param>
        /// <param name="minPrice">Mức giá tối thiểu</param>
        /// <param name="maxPrice">Mức giá tối đa</param>
        /// <param name="sortBy">Tiêu chí sắp xếp: price_asc, price_desc, new</param>
        /// <returns>Danh sách sản phẩm phù hợp</returns>
        [AllowAnonymous] // Cho phép React truy cập không cần đăng nhập Admin
        [HttpGet("api/products")]
        public async Task<IActionResult> GetProductsApi(
            [FromQuery] string? category,
            [FromQuery] string? search,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? sortBy)
        {
            // 1. Chỉ lấy các sản phẩm đang được phép hiển thị (IsActive = true)
            var query = _context.Products
                .Include(p => p.CategoryProduct)
                .Where(p => p.IsActive)
                .AsQueryable();

            // 2. Lọc thông minh: Bao gồm cả danh mục cha và các danh mục con của nó
            if (!string.IsNullOrEmpty(category) && category != "Tất cả")
            {
                var targetCategory = await _context.CategoryProducts
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == category.ToLower());

                if (targetCategory != null)
                {
                    // Lấy ID của danh mục được chọn và TẤT CẢ ID của các danh mục con thuộc về nó
                    var validCategoryIds = await _context.CategoryProducts
                        .Where(c => c.Id == targetCategory.Id || c.ParentId == targetCategory.Id)
                        .Select(c => c.Id)
                        .ToListAsync();

                    query = query.Where(p => validCategoryIds.Contains(p.CategoryProductId));
                }
            }

            // 3. Lọc theo từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(lowerSearch) || 
                                         (p.Description != null && p.Description.ToLower().Contains(lowerSearch)));
            }

            // 4. Lọc theo khoảng giá
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // 5. Xử lý sắp xếp
            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "price_asc":
                        query = query.OrderBy(p => p.Price);
                        break;
                    case "price_desc":
                        query = query.OrderByDescending(p => p.Price);
                        break;
                    case "new":
                    default:
                        query = query.OrderByDescending(p => p.CreatedDate);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(p => p.CreatedDate); // Mặc định sản phẩm mới lên đầu
            }

            // ĐÃ NÂNG CẤP: Map thêm CategoryImageUrl để React hiển thị Logo
            var productList = await query
                .Select(p => new {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.StockQuantity,
                    p.ImageUrl,
                    CategoryName = p.CategoryProduct != null ? p.CategoryProduct.Name : "Chưa phân loại",
                    CategoryImageUrl = p.CategoryProduct != null ? p.CategoryProduct.ImageUrl : null // Dòng ăn tiền ở đây!
                }).ToListAsync();

            return Ok(productList);
        }

        [AllowAnonymous]
        [HttpGet("api/products/{id}")]
        public async Task<IActionResult> GetProductDetailsApi(int id)
        {
            // Khách hàng chỉ xem được chi tiết nếu sản phẩm đó IsActive = true
            var product = await _context.Products
                .Include(p => p.CategoryProduct) // Include thêm để lấy được ImageUrl của Danh mục
                .Where(p => p.Id == id && p.IsActive)
                .Select(p => new {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.ImageUrl,
                    p.StockQuantity,
                    CategoryId = p.CategoryProductId,
                    CategoryName = p.CategoryProduct != null ? p.CategoryProduct.Name : "Chưa phân loại",
                    CategoryImageUrl = p.CategoryProduct != null ? p.CategoryProduct.ImageUrl : null, // Thêm ở Detail luôn cho đồng bộ
                    p.CreatedDate,
                    TotalSold = _context.OrderDetails.Where(od => od.ProductId == p.Id).Sum(od => (int?)od.Quantity) ?? 0
                }).FirstOrDefaultAsync();

            return product == null ? NotFound() : Ok(product);
        }

        [AllowAnonymous]
        [HttpGet("api/products/hot")]
        public async Task<IActionResult> GetHotProductsApi([FromQuery] int limit = 4)
        {
            var hotProducts = await _context.Products
                .Include(p => p.CategoryProduct)
                .Where(p => p.IsActive)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.StockQuantity,
                    p.ImageUrl,
                    CategoryName = p.CategoryProduct != null ? p.CategoryProduct.Name : "Chưa phân loại",
                    CategoryImageUrl = p.CategoryProduct != null ? p.CategoryProduct.ImageUrl : null,
                    TotalSold = _context.OrderDetails.Where(od => od.ProductId == p.Id).Sum(od => (int?)od.Quantity) ?? 0
                })
                // Chỉ lấy những sản phẩm có lượt mua > 0, nếu muốn hiển thị cả chưa mua thì bỏ Where này đi
                // .Where(p => p.TotalSold > 0) 
                .OrderByDescending(p => p.TotalSold)
                .Take(limit)
                .ToListAsync();

            return Ok(hotProducts);
        }

        [AllowAnonymous]
        [HttpGet("api/products/{id}/related")]
        public async Task<IActionResult> GetRelatedProductsApi(int id, [FromQuery] int limit = 4)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            var relatedProducts = await _context.Products
                .Include(p => p.CategoryProduct)
                .Where(p => p.IsActive && p.Id != id && p.CategoryProductId == product.CategoryProductId)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.StockQuantity,
                    p.ImageUrl,
                    CategoryName = p.CategoryProduct != null ? p.CategoryProduct.Name : "Chưa phân loại"
                })
                .OrderByDescending(p => p.Id) // Hoặc order by random
                .Take(limit)
                .ToListAsync();

            return Ok(relatedProducts);
        }
    }
}