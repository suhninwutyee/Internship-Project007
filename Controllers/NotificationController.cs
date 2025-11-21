//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using ProjectManagementSystem.DBModels;
//using ProjectManagementSystem.Models;
//using System.Linq;
//using System.Threading.Tasks;

//public class NotificationController : Controller
//{
//    private readonly PMSDbContext _context;

//    public NotificationController(PMSDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<IActionResult> GetNotifications()
//    {
//        var notifications = await _context.Notifications
//            .Include(n => n.ProjectPk)
//            .Where(n => n.IsRead == false)
//            .OrderByDescending(n => n.CreatedAt)
//            .Take(5)
//            .ToListAsync();

//        return PartialView("_NotificationDropdown", notifications);
//    }

    

//    [HttpPost]
//    public async Task<IActionResult> MarkAsRead(int id)
//    {
//        var notification = await _context.Notifications.FindAsync(id);
//        if (notification == null)
//        {
//            return NotFound(new { success = false });
//        }

//        notification.IsRead = true;
//        await _context.SaveChangesAsync();

//        var unreadCount = await _context.Notifications.CountAsync(n => n.IsRead == false);
//        return Json(new { success = true, unreadCount });
//    }

//    [HttpGet]
//    public async Task<int> GetNotificationCount()
//    {
//        return await _context.Notifications.CountAsync(n => n.IsRead == false);
//    }

//    [HttpPost]
//    public async Task<IActionResult> Delete(int id)
//    {
//        try
//        {
//            var notification = await _context.Notifications.FindAsync(id);
//            if (notification == null)
//            {
//                return NotFound();
//            }

//            _context.Notifications.Remove(notification);
//            await _context.SaveChangesAsync();

//            var unreadCount = await _context.Notifications.CountAsync(n => n.IsRead == false);
//            return Json(new { success = true, unreadCount });
//        }
//        catch (System.Exception ex)
//        {
//            return StatusCode(500, new { success = false, message = ex.Message });
//        }
//    }
//}