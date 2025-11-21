//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using ProjectManagementSystem.DBModels;
//using ProjectManagementSystem.Models;
//using System.Linq;
//using System.Security.Claims;
//using System.Threading.Tasks;

//namespace ProjectManagementSystem.Controllers
//{
//    public class NotificationsController : Controller
//    {
//        private readonly PMSDbContext _context;

//        public NotificationsController(PMSDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<IActionResult> Index()
//        {

//            var rollNumber = HttpContext.Session.GetString("RollNumber");
//            if (string.IsNullOrEmpty(rollNumber))
//            {
//                return RedirectToAction("Login", "StudentLogin");
//            }
//            var student = await _context.Students
//                .Include(s => s.EmailPk)
//                .FirstOrDefaultAsync(s => s.EmailPk.RollNumber == rollNumber);

//            if (student == null)
//                return NotFound();

//            var notifications = await _context.Notifications
//                .Where(n => n.UserId == student.StudentPkId)
//                .OrderByDescending(n => n.CreatedAt)
//                .ToListAsync();

//            return View(notifications);
//        }

//        [HttpPost]
//        public async Task<IActionResult> MarkAsRead(int id)
//        {
//            var notification = await _context.Notifications.FindAsync(id);
//            if (notification != null)
//            {
//                notification.IsRead = true;
//                await _context.SaveChangesAsync();
//            }

//            return Ok();
//        }

//        public async Task<IActionResult> GetUnreadCount()
//        {

//            var rollNumber = HttpContext.Session.GetString("RollNumber");
//            if (string.IsNullOrEmpty(rollNumber))
//                return Json(0);

//            var student = await _context.Students
//                .Include(s => s.EmailPk)
//                .FirstOrDefaultAsync(s => s.EmailPk.RollNumber == rollNumber);

//            if (student == null)
//                return Json(0);

//            var count = await _context.Notifications
//                .CountAsync(n => n.UserId == student.StudentPkId && n.IsRead == false);

//            return Json(count);
//        }

//        [HttpPost]
//        public async Task<IActionResult> MarkAllAsRead()
//        {
//            var rollNumber = HttpContext.Session.GetString("RollNumber");
//            if (string.IsNullOrEmpty(rollNumber))
//                return Json(new { success = false, count = 0 });

//            var student = await _context.Students
//                .Include(s => s.EmailPk)
//                .FirstOrDefaultAsync(s => s.EmailPk.RollNumber == rollNumber);

//            if (student == null)
//                return Json(new { success = false, count = 0 });

//            var notifications = await _context.Notifications
//                .Where(n => n.UserId == student.StudentPkId && n.IsRead == false)
//                .ToListAsync();

//            foreach (var notif in notifications)
//                notif.IsRead = true;

//            await _context.SaveChangesAsync();
//            return Json(new { success = true, count = notifications.Count });
//        }

//    }
//}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.DBModels;
using ProjectManagementSystem.Models;
using System.Linq;
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

        // ---------------------
        // Student Notifications
        // ---------------------
        public async Task<IActionResult> IndexStudent()
        {
            var userId = HttpContext.Session.GetInt32("StudentPkId");
            if (userId == null)
                return RedirectToAction("Login", "StudentLogin");

            var notifications = await _context.Notifications
                .Include(n => n.ProjectPk)
                .Where(n => n.UserId == userId &&
                            n.IsDeleted==false &&
                            (n.NotificationType == "Announcement" || n.NotificationType == "Response"))
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationViewModel
                {
                    Id = n.NotificationPkId,
                    Title = n.Title ?? "",
                    Message = n.Message ?? "",
                    IsRead = n.IsRead ?? false,
                    CreatedAt = n.CreatedAt ?? System.DateTime.Now,
                    ProjectId = n.ProjectPkId,
                    NotificationType = n.NotificationType ?? ""
                })
                .ToListAsync();

            return View("IndexStudent", notifications);
        }

        // ---------------------
        // Teacher Notifications
        // ---------------------
        public async Task<IActionResult> IndexTeacher()
        {
            var userId = HttpContext.Session.GetInt32("StudentPkId");
            if (userId == null)
                return RedirectToAction("Login", "StudentLogin");

            var notifications = await _context.Notifications
                .Include(n => n.ProjectPk)
                .Where(n => n.UserId == userId &&
                            n.IsDeleted==false &&
                            n.NotificationType == "ProjectSubmitted")
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationViewModel
                {
                    Id = n.NotificationPkId,
                    Title = n.Title ?? "",
                    Message = n.Message ?? "",
                    IsRead = n.IsRead ?? false,
                    CreatedAt = n.CreatedAt ?? System.DateTime.Now,
                    ProjectId = n.ProjectPkId,
                    NotificationType = n.NotificationType ?? ""
                })
                .ToListAsync();

            return View("IndexTeacher", notifications);
        }

        // ---------------------
        // Shared Actions
        // ---------------------
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

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = HttpContext.Session.GetInt32("UserPkId");
            if (userId == null)
                return Json(new { success = false, count = 0 });

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && n.IsRead == false && n.IsDeleted == false)
                .ToListAsync();

            foreach (var notif in notifications)
                notif.IsRead = true;

            await _context.SaveChangesAsync();
            return Json(new { success = true, count = notifications.Count });
        }

        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = HttpContext.Session.GetInt32("UserPkId");
            if (userId == null)
                return Json(0);

            var count = await _context.Notifications
                .Where(n => n.UserId == userId && n.IsRead == false && n.IsDeleted == false)
                .CountAsync();

            return Json(count);
        }
    }
}
