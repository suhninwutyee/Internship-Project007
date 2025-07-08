using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class EmailController : Controller
{
    private readonly ApplicationDbContext _context;

    public EmailController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Email
    public async Task<IActionResult> Index()
    {
        return View(await _context.Emails.Where(e => !e.IsDeleted).ToListAsync());
    }

    // GET: Email/Create (Single)
    public IActionResult Create()
    {
        return View();
    }

    // POST: Email/Create (Single)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("EmailAddress,RollNumber")] Email email)
    {
        if (ModelState.IsValid)
        {
            email.Class = "Final Year";
            email.CreatedDate = DateTimeOffset.Now;
            _context.Add(email);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(email);
    }

    // GET: Email/UploadBulk
    public IActionResult UploadBulk()
    {
        return View();
    }

    // POST: Email/UploadBulk
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadBulk(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please upload a CSV file.";
            return View();
        }

        var emails = new List<Email>();
        using (var stream = new StreamReader(file.OpenReadStream()))
        {
            string line;
            int lineNumber = 0;
            while ((line = await stream.ReadLineAsync()) != null)
            {
                lineNumber++;
                if (lineNumber == 1) continue; // Skip header

                var parts = line.Split(',');
                if (parts.Length < 2) continue;

                var emailAddress = parts[0].Trim();
                var rollNumber = parts[1].Trim();

                if (string.IsNullOrWhiteSpace(emailAddress) || string.IsNullOrWhiteSpace(rollNumber))
                    continue;

                emails.Add(new Email
                {
                    EmailAddress = emailAddress,
                    RollNumber = rollNumber,
                    Class = "Final Year",
                    CreatedDate = DateTimeOffset.Now
                });
            }
        }

        if (emails.Any())
        {
            await _context.Emails.AddRangeAsync(emails);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"{emails.Count} student emails uploaded successfully.";
        }
        else
        {
            TempData["Error"] = "No valid email entries found in the file.";
        }

        return View();
    }


[HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditInline(int id, [Bind("EmailAddress,RollNumber,Class")] Email model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(ms => ms.Value.Errors.Any())
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            return BadRequest(errors);
        }

        var existing = _context.Emails.FirstOrDefault(e => e.Email_PkId == id);
        if (existing == null)
        {
            return NotFound();
        }

        existing.EmailAddress = model.EmailAddress;
        existing.RollNumber = model.RollNumber;
        existing.Class = model.Class;
        _context.SaveChanges();

        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteInline(int id)
    {
        var email = _context.Emails.Find(id);
        if (email == null)
        {
            return NotFound();
        }

        _context.Emails.Remove(email);
        _context.SaveChanges();

        return Json(new { success = true });
    }


    private bool EmailExists(int id)
    {
        return _context.Emails.Any(e => e.Email_PkId == id && !e.IsDeleted);
    }
}