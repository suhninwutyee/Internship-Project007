using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.DBModels;

public class NotificationCountViewComponent : ViewComponent
{
    private readonly PMSDbContext _context;

    public NotificationCountViewComponent(PMSDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var count = await _context.Notifications
            .CountAsync(n => n.IsRead == false);
        return Content(count.ToString());
    }
}