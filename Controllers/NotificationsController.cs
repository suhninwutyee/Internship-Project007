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
                            n.IsDeleted == false &&
                            (n.NotificationType == "Announcement" ||
                             n.NotificationType == "Response" ||
                             n.NotificationType == "Schedule" ||
                             n.NotificationType == "Meeting"))
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationViewModel
                {
                    Id = n.NotificationPkId,
                    Title = n.Title ?? "",
                    Message = n.Message ?? "",
                    IsRead = n.IsRead ?? false,
                    CreatedAt = n.CreatedAt ?? DateTime.Now,
                    ProjectId = n.ProjectPkId,
                    ProjectName = n.ProjectPk.ProjectName ?? "",
                    NotificationType = n.NotificationType ?? "",
                    //DeadlineStatus = (n.NotificationType == "Meeting" && n.ProjectPk.MeetingTime != null)
                    //                 ? GetDeadlineStatus(n.ProjectPk.MeetingTime.Value)
                    //                 : ""
                })
                .ToListAsync();

            return View("IndexStudent", notifications);
        }

        //Helper method to calculate deadline status
        private string GetDeadlineStatus(DateTime meetingTime)
        {
            var hoursLeft = (meetingTime - DateTime.Now).TotalHours;
            if (hoursLeft <= 1 && hoursLeft > 0)
                return "Starting Soon";
            else if (hoursLeft <= 24 && hoursLeft > 1)
                return "Tomorrow";
            else if (hoursLeft <= 0)
                return "Missed";
            else
                return "";
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
                            n.IsDeleted == false &&
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
        // Mark a single notification as read
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

        // Mark all notifications as read
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = HttpContext.Session.GetInt32("StudentPkId"); // use correct session key
            if (userId == null)
                return Json(new { success = false, count = 0 });

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !(n.IsRead ?? false) && !(n.IsDeleted ?? false))
                .ToListAsync();

            foreach (var notif in notifications)
                notif.IsRead = true;

            await _context.SaveChangesAsync();
            return Json(new { success = true, count = notifications.Count });
        }

        // Get count of unread notifications
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = HttpContext.Session.GetInt32("StudentPkId");
            if (userId == null) return Json(0);

            var count = await _context.Notifications
                .Where(n => n.UserId == userId && !(n.IsRead ?? false) && !(n.IsDeleted ?? false))
                .CountAsync();

            return Json(count);
        }

        // Get count of read notifications
        public async Task<IActionResult> GetReadCount()
        {
            var userId = HttpContext.Session.GetInt32("StudentPkId");
            if (userId == null) return Json(0);

            var count = await _context.Notifications
                .Where(n => n.UserId == userId && (n.IsRead ?? false) && !(n.IsDeleted ?? false))
                .CountAsync();

            return Json(count);
        }

        [HttpPost]
        public async Task<IActionResult> AssignSchedule([FromBody] AssignDto dto)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectMembers)
                .FirstOrDefaultAsync(p => p.ProjectPkId == dto.ProjectId);

            if (project == null) return Json(new { success = false, message = "Project not found." });

            if (!DateTime.TryParse(dto.DateTime, out var parsed))
                return Json(new { success = false, message = "Invalid date/time." });

            project.ScheduleTime = parsed;
            await _context.SaveChangesAsync();

            // Notify all members
            foreach (var m in project.ProjectMembers)
            {
                var notif = new Notification
                {
                    UserId = m.StudentPkId,
                    ProjectPkId = project.ProjectPkId,
                    Title = "Schedule Assigned",
                    Message = $"Schedule: {parsed:dd MMM yyyy, hh:mm tt}",
                    NotificationType = "Schedule",
                    IsRead = false,
                    IsDeleted = false,
                    CreatedAt = DateTime.Now
                };
                _context.Notifications.Add(notif);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Schedule assigned and members notified." });
        }

        // -----------------------------
        // Assign Meeting
        // -----------------------------
        [HttpPost]
        public async Task<IActionResult> AssignMeeting([FromBody] AssignDto dto)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectMembers)
                .FirstOrDefaultAsync(p => p.ProjectPkId == dto.ProjectId);

            if (project == null) return Json(new { success = false, message = "Project not found." });

            if (!DateTime.TryParse(dto.DateTime, out var parsed))
                return Json(new { success = false, message = "Invalid date/time." });

            project.MeetingTime = parsed;
            await _context.SaveChangesAsync();

            // Notify all members
            foreach (var m in project.ProjectMembers)
            {
                var notif = new Notification
                {
                    UserId = m.StudentPkId,
                    ProjectPkId = project.ProjectPkId,
                    Title = "Meeting Scheduled",
                    Message = $"Meeting: {parsed:dd MMM yyyy, hh:mm tt}",
                    NotificationType = "Meeting",
                    IsRead = false,
                    IsDeleted = false,
                    CreatedAt = DateTime.Now
                };
                _context.Notifications.Add(notif);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Meeting assigned and members notified." });
        }

        // -----------------------------
        // DTO Class
        // -----------------------------
        public class AssignDto
        {
            public int ProjectId { get; set; }
            public string DateTime { get; set; } = string.Empty; // sent from client
        }
    }

}

