//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using ProjectManagementSystem.Data;

//[Authorize(Roles = "Admin,Teacher")]
//public class AnnouncementController : Controller
//{
//    private readonly ApplicationDbContext _context;

//    public AnnouncementController(ApplicationDbContext context)
//    {
//        _context = context;
//    }

//    public IActionResult Edit()
//    {
//        var announcement = _context.Announcements.FirstOrDefault() ?? new Announcement();
//        return View(announcement);
//    }

//    [HttpPost]
//    [ValidateAntiForgeryToken]
//    public IActionResult Edit(Announcement model)
//    {
//        if (ModelState.IsValid)
//        {
//            // Ensure we only have one announcement
//            model.AnnouncementId = 1;

//            if (_context.Announcements.Any())
//            {
//                _context.Update(model);
//            }
//            else
//            {
//                _context.Add(model);
//            }

//            _context.SaveChanges();
//            TempData["SuccessMessage"] = "Announcement updated successfully";
//            return RedirectToAction("Dashboard", "Teacher");
//        }
//        return View(model);
//    }
//}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Teacher")] // Teacher + Admin only
    public class AnnouncementController : Controller
    {
        private readonly PMSDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AnnouncementController(PMSDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: /Announcement
        public IActionResult Index()
        {
            var announcements = _context.Announcements
                .OrderByDescending(a => a.CreatedDate)
                .ToList();
            return View(announcements);
        }

        // GET: /Announcement/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Announcement/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(Announcement model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        // Handle file upload
        //        if (model.Attachment != null)
        //        {
        //            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads/announcements");
        //            if (!Directory.Exists(uploadsFolder))
        //                Directory.CreateDirectory(uploadsFolder);

        //            string uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(model.Attachment.FileName);
        //            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        //            using (var stream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await model.Attachment.CopyToAsync(stream);
        //            }

        //            model.FilePath = "/uploads/announcements/" + uniqueFileName;
        //        }

        //        model.CreatedDate = DateTime.Now;
        //        _context.Announcements.Add(model);
        //        await _context.SaveChangesAsync();

        //        TempData["Success"] = "Announcement created successfully!";
        //        return RedirectToAction(nameof(Index));
        //    }

        //    return View(model);
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Announcement model)
        {
            if (ModelState.IsValid)
            {
                if (model.Attachment != null)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads/announcements");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.Attachment.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Attachment.CopyToAsync(stream);
                    }

                    model.FilePath = "/uploads/announcements/" + uniqueFileName;
                }

                model.CreatedDate = DateTime.Now;
                _context.Add(model);
                await _context.SaveChangesAsync();

                // ===========================
                // Auto-create notifications for all students
                // ===========================
                var allStudents = await _context.Students.ToListAsync();
                foreach (var student in allStudents)
                {
                    var notification = new Notification
                    {
                        UserId = student.Student_pkId,
                        Title = "New Announcement",
                        Message = model.Title, // Short message
                        NotificationType = "Announcement",
                        CreatedAt = DateTime.Now,
                        Project_pkId = null // If announcement is related to project, link it here
                    };
                    _context.Notifications.Add(notification);
                }
                await _context.SaveChangesAsync();
                // ===========================

                TempData["Success"] = "Announcement created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }


        // GET: /Announcement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null) return NotFound();

            return View(announcement);
        }

        // POST: /Announcement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Announcement model)
        {
            if (id != model.AnnouncementId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle new file upload
                    if (model.Attachment != null)
                    {
                        string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads/announcements");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        string uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(model.Attachment.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.Attachment.CopyToAsync(stream);
                        }

                        // Delete old file if exists
                        if (!string.IsNullOrEmpty(model.FilePath))
                        {
                            string oldFile = Path.Combine(_env.WebRootPath, model.FilePath.TrimStart('/').Replace("/", "\\"));
                            if (System.IO.File.Exists(oldFile))
                                System.IO.File.Delete(oldFile);
                        }

                        model.FilePath = "/uploads/announcements/" + uniqueFileName;
                    }

                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Announcement updated successfully!";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error updating announcement: " + ex.Message;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: /Announcement/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null) return NotFound();

            return View(announcement);
        }

        // POST: /Announcement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement != null)
            {
                // Delete attached file if exists
                if (!string.IsNullOrEmpty(announcement.FilePath))
                {
                    string file = Path.Combine(_env.WebRootPath, announcement.FilePath.TrimStart('/').Replace("/", "\\"));
                    if (System.IO.File.Exists(file))
                        System.IO.File.Delete(file);
                }

                _context.Announcements.Remove(announcement);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Announcement deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Announcement/Detail/5
        [AllowAnonymous]
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null)
                return NotFound();

            var announcement = await _context.Announcements
                .FirstOrDefaultAsync(a => a.AnnouncementId == id);

            if (announcement == null)
                return NotFound();

            return View(announcement);
        }

        // Student view: only active announcements
        [AllowAnonymous]
        public IActionResult StudentView()
        {
            var activeAnnouncements = _context.Announcements
                .AsEnumerable() // important: bring to memory to use IsActive property
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.StartDate)
                .ToList();

            return View(activeAnnouncements);
        }
    }
}
