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



//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using ProjectManagementSystem.Data;
//using ProjectManagementSystem.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace ProjectManagementSystem.Controllers
//{
//    public class NotificationsController : Controller
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly ILogger<NotificationsController> _logger;

//        public NotificationsController(
//            ApplicationDbContext context,
//            ILogger<NotificationsController> logger)
//        {
//            _context = context;
//            _logger = logger;
//        }

//        [HttpGet]
//        public async Task<IActionResult> Index()
//        {
//            try
//            {
//                var rollNumber = HttpContext.Session.GetString("RollNumber");
//                if (string.IsNullOrEmpty(rollNumber))
//                {
//                    return RedirectToAction("Login", "StudentLogin");
//                }

//                var student = await GetCurrentStudentAsync(rollNumber);
//                if (student == null)
//                {
//                    return NotFound("Student not found");
//                }

//                var notifications = await GetNotificationsForStudentAsync(student.Student_pkId);
//                return View(notifications);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error loading notifications");
//                return View("Error");
//            }
//        }

//        //[HttpPost]
//        //[ValidateAntiForgeryToken]
//        //public async Task<IActionResult> MarkAsRead(int id)
//        //{
//        //    try
//        //    {
//        //        var notification = await _context.Notifications.FindAsync(id);
//        //        if (notification == null)
//        //        {
//        //            return NotFound();
//        //        }

//        //        notification.IsRead = true;
//        //        notification.CreatedDate = DateTime.UtcNow;
//        //        await _context.SaveChangesAsync();

//        //        return Ok(new { success = true });
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        _logger.LogError(ex, $"Error marking notification {id} as read");
//        //        return StatusCode(500, new { error = "An error occurred" });
//        //    }
//        //}
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
//        //[HttpPost]
//        //[ValidateAntiForgeryToken]
//        //public async Task<IActionResult> MarkAllAsRead()
//        //{
//        //    try
//        //    {
//        //        var rollNumber = HttpContext.Session.GetString("RollNumber");
//        //        if (string.IsNullOrEmpty(rollNumber))
//        //        {
//        //            return Unauthorized();
//        //        }

//        //        var student = await GetCurrentStudentAsync(rollNumber);
//        //        if (student == null)
//        //        {
//        //            return NotFound("Student not found");
//        //        }

//        //        var unreadNotifications = await _context.Notifications
//        //            .Where(n => n.UserId == student.Student_pkId && !n.IsRead)
//        //            .ToListAsync();

//        //        foreach (var notification in unreadNotifications)
//        //        {
//        //            notification.IsRead = true;
//        //            notification.CreatedDate = DateTime.UtcNow;
//        //        }

//        //        await _context.SaveChangesAsync();

//        //        return Ok(new { success = true, count = unreadNotifications.Count });
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        _logger.LogError(ex, "Error marking all notifications as read");
//        //        return StatusCode(500, new { error = "An error occurred" });
//        //    }
//        //}

//        [HttpGet]
//        public async Task<IActionResult> GetUnreadCount()
//        {
//            try
//            {
//                var rollNumber = HttpContext.Session.GetString("RollNumber");
//                if (string.IsNullOrEmpty(rollNumber))
//                {
//                    return Json(new { count = 0 });
//                }

//                var student = await GetCurrentStudentAsync(rollNumber);
//                if (student == null)
//                {
//                    return Json(new { count = 0 });
//                }

//                var count = await _context.Notifications
//                    .CountAsync(n => n.UserId == student.Student_pkId && !n.IsRead);

//                return Json(new { count });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting unread notification count");
//                return Json(new { count = 0 });
//            }
//        }

//        private async Task<Student> GetCurrentStudentAsync(string rollNumber)
//        {
//            return await _context.Students
//                .Include(s => s.Email)
//                .FirstOrDefaultAsync(s => s.Email.RollNumber == rollNumber);
//        }

//        private async Task<List<NotificationViewModel>> GetNotificationsForStudentAsync(int studentId)
//        {
//            return await _context.Notifications
//                .Where(n => n.UserId == studentId)
//                .OrderByDescending(n => n.CreatedDate)
//                .Select(n => new NotificationViewModel
//                {
//                    Notification_pkId = n.Notification_pkId,
//                    Title = n.Title,
//                    Message = n.Message,
//                    CreatedDate = n.CreatedDate,
//                    IsRead = n.IsRead,
//                    NotificationType = n.NotificationType,
//                    RelatedEntityId = n.UserId
//                })
//                .ToListAsync();
//        }
//    }
//}