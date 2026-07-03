using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace CMS.Backend.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,Editor")] // Editor được phép quản lý Banner
    public class BannerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public BannerController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var banners = await _context.Banners.OrderBy(b => b.DisplayOrder).ToListAsync();
            return View(banners);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Banner banner, IFormFile? uploadImage)
        {
            if (ModelState.IsValid)
            {
                if (uploadImage != null)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "banners");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(uploadImage.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadImage.CopyToAsync(fileStream);
                    }
                    banner.ImageUrl = "/uploads/banners/" + uniqueFileName;
                }

                banner.CreatedDate = DateTime.Now;
                _context.Banners.Add(banner);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(banner);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return NotFound();
            return View(banner);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Banner banner, IFormFile? uploadImage)
        {
            if (id != banner.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (uploadImage != null)
                    {
                        string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "banners");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                        
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(uploadImage.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await uploadImage.CopyToAsync(fileStream);
                        }
                        banner.ImageUrl = "/uploads/banners/" + uniqueFileName;
                    }

                    _context.Update(banner);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BannerExists(banner.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(banner);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner != null)
            {
                _context.Banners.Remove(banner);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool BannerExists(int id)
        {
            return _context.Banners.Any(e => e.Id == id);
        }
    }
}
