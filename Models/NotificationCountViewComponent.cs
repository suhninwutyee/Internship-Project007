// File: ViewComponents/NotificationCountViewComponent.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProjectManagementSystem.ViewComponents
{
    [ViewComponent(Name = "NotificationCount")]
    public class NotificationCountViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public NotificationCountViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Get user ID based on your authentication system
            var userId = UserClaimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Content("0");

            try
            {
                // If your UserId is an int in the database:
                if (!int.TryParse(userId, out int userIdInt))
                    return Content("0");

                var count = await _context.Notifications
                    .CountAsync(n => n.UserId == userIdInt && !n.IsRead);

                return Content(count.ToString());
            }
            catch (Exception ex)
            {
                // Log error if needed
                return Content("0");
            }
        }
    }
}