using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserAuthApi.Data;
using UserAuthApi.Models; // Message modelin burada olmalı

namespace UserAuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly DataContext _context;

        public MessagesController(DataContext context)
        {
            _context = context;
        }

        // MESAJ GÖNDERME
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] Message msg)
        {
            if (msg == null) return BadRequest();

            msg.SentAt = DateTime.Now;
            _context.Messages.Add(msg);
            await _context.SaveChangesAsync();
            return Ok("Mesaj iletildi!");
        }

        // GELEN KUTUSU (Kullanıcıya gelen mesajları listeler)
        [HttpGet("inbox/{username}")]
        public async Task<IActionResult> GetInbox(string username)
        {
            var messages = await _context.Messages
                .Where(m => m.ReceiverUsername == username)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            return Ok(messages);
        }
        // GİDEN KUTUSU (Kullanıcının başkalarına sorduğu sorular)
        [HttpGet("outbox/{username}")]
        public async Task<IActionResult> GetOutbox(string username)
        {
            var messages = await _context.Messages
                .Where(m => m.SenderUsername == username)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            return Ok(messages);
        }
        // api/messages/chat/{username}
        [HttpGet("chat/{username}")]
        public async Task<IActionResult> GetChatHistory(string username)
        {
            var messages = await _context.Messages
                .Where(m => m.ReceiverUsername == username || m.SenderUsername == username)
                .OrderByDescending(m => m.SentAt) // En yeni en üstte
                .ToListAsync();

            return Ok(messages);
        }
        [HttpPost("markAsRead/{receiver}/{sender}")]
        public async Task<IActionResult> MarkAsRead(string receiver, string sender)
        {
            var unreadMessages = await _context.Messages
                .Where(m => m.ReceiverUsername == receiver && m.SenderUsername == sender && !m.IsRead)
                .ToListAsync();

            unreadMessages.ForEach(m => m.IsRead = true);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}