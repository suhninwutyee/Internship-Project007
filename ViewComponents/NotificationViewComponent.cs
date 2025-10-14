using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;

public class NotificationViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public NotificationViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    //public async Task<IViewComponentResult> InvokeAsync()
    //{
    //    var notifications = await _context.Notifications
    //        .Include(n => n.Project)
    //        .Where(n => !n.IsRead)
    //        .OrderByDescending(n => n.CreatedAt)
    //        .Take(5)
    //        .Select(n => new NotificationViewModel
    //        {
    //            Id = n.Notification_pkId,
    //            Message = n.Message,
    //            CreatedAt = (DateTime)n.CreatedAt,
    //            ProjectId = (int)n.Project_pkId,
    //            ProjectName = n.Project.ProjectName,
    //            IsRead = n.IsRead,
    //        })
    //        .ToListAsync();

    //    return View(notifications);
    //}

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var notifications = await _context.Notifications
            .Include(n => n.Project)
            .Where(n => !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .Take(5)
            .Select(n => new NotificationViewModel
            {
                Id = n.Notification_pkId,
                Message = n.Message,
                CreatedAt = (DateTime)n.CreatedAt, // NULL safe
                ProjectId = n.Project_pkId ?? 0,          // NULL safe
                ProjectName = n.Project != null ? n.Project.ProjectName : "No Project",
                IsRead = n.IsRead,
            })
            .ToListAsync();

        return View(notifications);
    }
}

public class NotificationViewModel
{
    public int Id { get; set; }
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? ProjectId { get; set; }
    public string ProjectName { get; set; }
    public bool IsRead { get; set; }
}