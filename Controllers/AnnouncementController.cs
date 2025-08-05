using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.Data;

[Authorize(Roles = "Admin,Teacher")]
public class AnnouncementController : Controller
{
    private readonly ApplicationDbContext _context;

    public AnnouncementController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Edit()
    {
        var announcement = _context.Announcements.FirstOrDefault() ?? new Announcement();
        return View(announcement);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Announcement model)
    {
        if (ModelState.IsValid)
        {
            // Ensure we only have one announcement
            model.Id = 1;

            if (_context.Announcements.Any())
            {
                _context.Update(model);
            }
            else
            {
                _context.Add(model);
            }

            _context.SaveChanges();
            TempData["SuccessMessage"] = "Announcement updated successfully";
            return RedirectToAction("Dashboard", "Teacher");
        }
        return View(model);
    }
}