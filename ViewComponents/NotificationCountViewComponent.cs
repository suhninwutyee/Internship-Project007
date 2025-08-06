using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;

public class NotificationCountViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public NotificationCountViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var count = await _context.Notifications
            .CountAsync(n => !n.IsRead);
        return Content(count.ToString());
    }
}