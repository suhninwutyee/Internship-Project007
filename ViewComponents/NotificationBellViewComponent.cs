using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.DBModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ProjectManagementSystem.ViewComponents
{
    public class NotificationBellViewComponent : ViewComponent
    {
        private readonly PMSDbContext _context;

        public NotificationBellViewComponent(PMSDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string role)
        {
            var userId = HttpContext.Session.GetInt32("StudentPkId");
            if (userId == null) return View(new List<NotificationViewModel>());

            IQueryable<Notification> query = _context.Notifications
                .Include(n => n.ProjectPk)
                .Where(n => n.UserId == userId && n.IsDeleted == false);

            if (role == "Student")
            {
                query = query.Where(n => n.NotificationType == "Announcement" || n.NotificationType == "Response");
            }
            else
            {
                query = query.Where(n => n.NotificationType == "ProjectSubmitted");
            }

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .Select(n => new NotificationViewModel
                {
                    Id = n.NotificationPkId,
                    Message = n.Message,
                    CreatedAt = n.CreatedAt ?? DateTime.Now,
                    ProjectId = n.ProjectPkId,
                    ProjectName = n.ProjectPk != null ? n.ProjectPk.ProjectName : "No Project",
                    IsRead = n.IsRead
                })
                .ToListAsync();

            return View(notifications);
        }
    }
}
