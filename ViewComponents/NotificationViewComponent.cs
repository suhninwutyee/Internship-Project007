using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.DBModels;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectManagementSystem.ViewComponents
{
    public class NotificationViewComponent : ViewComponent 
    {
        private readonly PMSDbContext _context;

        public NotificationViewComponent(PMSDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var notifications = await _context.Notifications
                .Include(n => n.ProjectPk)
                .Where(n => n.IsRead==false)
                .Where(n => n.NotificationType == "ProjectSubmitted" && n.IsRead == false)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .Select(n => new NotificationViewModel
                {
                    Id = n.NotificationPkId,
                    Message = n.Message,
                    CreatedAt = (DateTime)n.CreatedAt, 
                    ProjectId = n.ProjectPkId ?? 0,         
                    ProjectName = n.ProjectPk != null ? n.ProjectPk.ProjectName : "No Project",
                    IsRead = n.IsRead,
                })
                .ToListAsync();

            return View(notifications);
        }
    }
}
