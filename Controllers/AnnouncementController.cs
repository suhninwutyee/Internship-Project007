using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;

[Authorize(Roles = "Admin,Teacher")]
public class AnnouncementController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AnnouncementController> _logger;

    public AnnouncementController(ApplicationDbContext context,
                                ILogger<AnnouncementController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Announcement/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View(new Announcement
        {
            Title = "New Announcement",

            BlocksSubmissions = false,
            StartDate = DateTime.Now,
            ExpiryDate = DateTime.Now.AddDays(7)
        });
    }


    // POST: Announcement/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Announcement announcement)
    {
        try
        {
            if (ModelState.IsValid)
            {
                announcement.CreatedDate = DateTime.Now;

                // Deactivate other announcements if this one is active
                if (announcement.IsActive)
                {
                    await DeactivateOtherAnnouncements();
                }

                _context.Add(announcement);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Announcement created successfully";
                return RedirectToAction("Dashboard", "Teacher");
            }
            return View(announcement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating announcement");
            TempData["ErrorMessage"] = "Error creating announcement";
            return View(announcement);
        }
    }

    // GET: Announcement/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var announcement = await _context.Announcements.FindAsync(id);
        if (announcement == null)
        {
            return NotFound();
        }
        return View(announcement);
    }

    // POST: Announcement/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Announcement announcement)
    {
        if (id != announcement.AnnouncementId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Deactivate other announcements if this one is active
                if (announcement.IsActive)
                {
                    await DeactivateOtherAnnouncements(announcement.AnnouncementId);
                }

                _context.Update(announcement);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Announcement updated successfully";
                return RedirectToAction("Dashboard", "Teacher");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating announcement");
                TempData["ErrorMessage"] = "Error updating announcement";
                return View(announcement);
            }
        }
        return View(announcement);
    }

    private async Task DeactivateOtherAnnouncements(int? currentAnnouncementId = null)
    {
        var activeAnnouncements = _context.Announcements
            .Where(a => a.IsActive);

        if (currentAnnouncementId.HasValue)
        {
            activeAnnouncements = activeAnnouncements
                .Where(a => a.AnnouncementId != currentAnnouncementId);
        }

        foreach (var ann in await activeAnnouncements.ToListAsync())
        {

            _context.Update(ann);
        }
    }
}