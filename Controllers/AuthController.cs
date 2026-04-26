using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserAuthApi.Models; // Models klasöründeki User sınıfını görmesi için
using UserAuthApi.Data;   // Data klasöründeki DataContext'i görmesi için

namespace UserAuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _context;

        public AuthController(DataContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDto request)
        {
            // Şifreyi şifreliyoruz (Hashleme)
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                PasswordHash = hashedPassword // Şifreli halini kaydediyoruz
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok("Kayıt başarılı kanka!");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null) return BadRequest("Kullanıcı yok kanka!");

            // Şifre kontrolü
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return BadRequest("Şifre yanlış!");
            }

            return Ok(new { username = user.Username });
        }

        [HttpGet("users")]
        public async Task<ActionResult<List<User>>> GetAllUsers()
        {
            // Veritabanındaki tüm kullanıcıları listeler (Şifreleri de gelir ama gerçekte gönderilmez, şimdilik böyle kalsın)
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        
        }
        [HttpDelete("delete/{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok("Kullanıcı başarıyla silindi.");
        }
        [HttpPost("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            // 1. Mevcut şifre kontrolü (Burası Verify ile olmalı)
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            {
                return BadRequest("Mevcut şifren yanlış kanka!");
            }

            // 2. YENİ ŞİFREYİ ŞİFRELEYEREK KAYDET (Hata buradaydı muhtemelen!)
            // Sakın user.PasswordHash = dto.NewPassword; YAPMA!
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            await _context.SaveChangesAsync();
            return Ok("Şifre güncellendi.");
        }

        // Dosyanın en altındaki DTO kısmına bunu da ekle:
        public class UpdatePasswordDto
        {
            public string Username { get; set; } = string.Empty;
            public string CurrentPassword { get; set; } = string.Empty; // İşte bu satırı ekle kanka!
            public string NewPassword { get; set; } = string.Empty;
        }

        // İstekleri karşılamak için basit veri transfer objesi (DTO)
        public class UserDto
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
    }
}