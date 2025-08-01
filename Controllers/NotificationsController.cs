using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProjectManagementSystem.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(ApplicationDbContext context)
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
                .Include(s => s.Email)
                .FirstOrDefaultAsync(s => s.Email.RollNumber == rollNumber);

            if (student == null)
                return NotFound();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == student.Student_pkId)
                .OrderByDescending(n => n.CreatedDate)
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
                .Include(s => s.Email)
                .FirstOrDefaultAsync(s => s.Email.RollNumber == rollNumber);

            if (student == null)
                return Json(0);

            var count = await _context.Notifications
                .CountAsync(n => n.UserId == student.Student_pkId && !n.IsRead);

            return Json(count);
        }
    }
}