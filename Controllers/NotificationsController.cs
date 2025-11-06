using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.DBModels;
using ProjectManagementSystem.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProjectManagementSystem.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly PMSDbContext _context;

        public NotificationsController(PMSDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get current student ID (you might need to adjust this based on your auth system)
            //var rollNumber = HttpContext.Session.GetString("RollNumber");
            //if (string.IsNullOrEmpty(rollNumber))
            //    return RedirectToAction("Login", "Account");
            var rollNumber = HttpContext.Session.GetString("RollNumber");
            if (string.IsNullOrEmpty(rollNumber))
            {
                return RedirectToAction("Login", "StudentLogin");
            }
            var student = await _context.Students
                .Include(s => s.EmailPk)
                .FirstOrDefaultAsync(s => s.EmailPk.RollNumber == rollNumber);

            if (student == null)
                return NotFound();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == student.StudentPkId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(notifications);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        public async Task<IActionResult> GetUnreadCount()
        {
            // Get current student ID
            var rollNumber = HttpContext.Session.GetString("RollNumber");
            if (string.IsNullOrEmpty(rollNumber))
                return Json(0);

            var student = await _context.Students
                .Include(s => s.EmailPk)
                .FirstOrDefaultAsync(s => s.EmailPk.RollNumber == rollNumber);

            if (student == null)
                return Json(0);

            var count = await _context.Notifications
                .CountAsync(n => n.UserId == student.StudentPkId && n.IsRead == false);

            return Json(count);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var rollNumber = HttpContext.Session.GetString("RollNumber");
            if (string.IsNullOrEmpty(rollNumber))
                return Json(new { success = false, count = 0 });

            var student = await _context.Students
                .Include(s => s.EmailPk)
                .FirstOrDefaultAsync(s => s.EmailPk.RollNumber == rollNumber);

            if (student == null)
                return Json(new { success = false, count = 0 });

            var notifications = await _context.Notifications
                .Where(n => n.UserId == student.StudentPkId && n.IsRead == false)
                .ToListAsync();

            foreach (var notif in notifications)
                notif.IsRead = true;

            await _context.SaveChangesAsync();
            return Json(new { success = true, count = notifications.Count });
        }

    }
}
