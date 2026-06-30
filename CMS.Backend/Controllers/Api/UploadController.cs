using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace CMS.Backend.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        [HttpPost("ckeditor")]
        public async Task<IActionResult> UploadImageCKEditor(IFormFile upload)
        {
            if (upload == null || upload.Length == 0)
            {
                return BadRequest(new { error = new { message = "Không có file nào được chọn." } });
            }

            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(upload.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await upload.CopyToAsync(fileStream);
                }

                var url = $"/images/uploads/{uniqueFileName}";
                
                // Trả về JSON chuẩn cho CKEditor (CKFinder / CKEditor 5 Upload Adapter format)
                return Ok(new
                {
                    uploaded = 1,
                    fileName = uniqueFileName,
                    url = url
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = new { message = ex.Message } });
            }
        }
    }
}
