using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using UserAuthApi.Models;
using UserAuthApi.Data;
using Microsoft.EntityFrameworkCore;

namespace UserAuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdsController : ControllerBase
    {
        private readonly DataContext _context;

        public AdsController(DataContext context)
        {
            _context = context;
        }

        // TÜM İLANLARI LİSTELEME
        [HttpGet("all")]
        public async Task<IActionResult> GetAllAds()
        {
            try
            {
                var ads = await _context.Ads.OrderByDescending(x => x.CreatedAt).ToListAsync();
                return Ok(ads);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // YENİ İLAN VE RESİM YÜKLEME (TEK METOT)
        [HttpPost("post")]
        public async Task<IActionResult> PostAd([FromForm] IFormCollection data, IFormFile? image)
        {
            try
            {
                string? dbImageUrl = null;

                if (image != null && image.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    dbImageUrl = "/uploads/" + fileName;
                }

                var ad = new Ad
                {
                    Title = data["title"].ToString() ?? "Başlıksız İlan",
                    Description = data["description"].ToString() ?? "",
                    Price = decimal.TryParse(data["price"], out decimal p) ? p : 0,
                    Category = data["category"].ToString() ?? "Diğer",
                    ImageUrl = dbImageUrl,
                    CreatedBy = data["createdBy"].ToString() ?? "Anonim",
                    UserId = int.TryParse(data["userId"], out int u) ? u : 1,
                    CreatedAt = DateTime.Now
                };

                _context.Ads.Add(ad);
                await _context.SaveChangesAsync();
                return Ok("İlan ve resim yüklendi! ✅");
            }
            catch (Exception ex)
            {
                return BadRequest($"Hata: {ex.Message}");
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateAd(int id, [FromForm] IFormCollection data, IFormFile? image)
        {
            var ad = await _context.Ads.FindAsync(id);
            if (ad == null) return NotFound("İlan bulunamadı.");

            // Yazıları güncelle
            ad.Title = data["title"].ToString() ?? ad.Title;
            ad.Price = decimal.TryParse(data["price"], out decimal p) ? p : ad.Price;

            // Eğer yeni bir resim seçildiyse onu kaydet
            if (image != null && image.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }
                ad.ImageUrl = "/uploads/" + fileName;
            }

            await _context.SaveChangesAsync();
            return Ok("Güncellendi");
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteAd(int id)
        {
            var ad = await _context.Ads.FindAsync(id);
            if (ad == null) return NotFound();

            _context.Ads.Remove(ad);
            await _context.SaveChangesAsync();
            return Ok("İlan silindi.");
        }
    }
}