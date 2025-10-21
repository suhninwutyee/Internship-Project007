using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementSystem.ViewComponents
{
    public class NotificationBellViewComponent : ViewComponent
    {
        private readonly PMSDbContext _context;

        public NotificationBellViewComponent(PMSDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var rollNumber = HttpContext.Session.GetString("RollNumber");

            if (string.IsNullOrEmpty(rollNumber))
                return View(Enumerable.Empty<ProjectManagementSystem.Models.Notification>());

            var student = await _context.Students
                .Include(s => s.Email)
                .FirstOrDefaultAsync(s => s.Email.RollNumber == rollNumber);

            if (student == null)
                return View(Enumerable.Empty<ProjectManagementSystem.Models.Notification>());

            var notifications = await _context.Notifications
                .Where(n => n.UserId == student.Student_pkId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View(notifications);
        }
    }
}
