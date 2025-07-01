using AutoOtpad.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoOtpad.Controllers
{
    public class MessageController : Controller
    {
        private readonly AppDbContext _context;

        public MessageController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? withUserId)
        {
            var currentUserId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");

            var users = await _context.Users
                .Where(u => u.Id != currentUserId)
                .ToListAsync();

            ViewBag.Users = users;
            ViewBag.WithUserId = withUserId;

            if (withUserId == null) return View(new List<Message>());

            var messages = await _context.Messages
                .Where(m =>
                    (m.SenderId == currentUserId && m.ReceiverId == withUserId) ||
                    (m.SenderId == withUserId && m.ReceiverId == currentUserId))
                .OrderBy(m => m.SentAt)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .ToListAsync();

            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int receiverId, string content)
        {
            var senderId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");

            if (senderId == 0 || receiverId == 0 || string.IsNullOrWhiteSpace(content))
                return RedirectToAction("Index", new { withUserId = receiverId });

            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                SentAt = DateTime.Now
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { withUserId = receiverId });
        }
    }
}
