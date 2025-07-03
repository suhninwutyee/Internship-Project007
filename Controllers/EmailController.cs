using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

[Authorize(Roles = "Admin")]
public class EmailController : Controller
{
    private readonly ApplicationDbContext _context;

    public EmailController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Email/Index
    public async Task<IActionResult> Index()
    {
        var emails = await _context.Emails
            .Where(e => !e.IsDeleted && e.Class == "FinalYear")
            .ToListAsync();

        return View(emails);
    }

    // POST: Email/Create (AJAX)
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] Email email)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        email.CreatedDate = DateTimeOffset.UtcNow;
        email.IsDeleted = false;
        email.Class = "FinalYear";  // Enforce FinalYear

        _context.Add(email);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Email added successfully." });
    }

    // POST: Email/Edit/5 (AJAX)
    [HttpPost]
    public async Task<IActionResult> Edit(int id, [FromForm] Email updatedEmail)
    {
        if (id != updatedEmail.Email_PkId)
            return BadRequest("ID mismatch.");

        var email = await _context.Emails.FindAsync(id);
        if (email == null || email.IsDeleted)
            return NotFound();

        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        email.EmailAddress = updatedEmail.EmailAddress;
        // Keep Class as FinalYear in case someone tries to change it
        email.Class = "FinalYear";

        _context.Update(email);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Email updated successfully." });
    }

    // POST: Email/DeleteConfirmed (AJAX)
    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var email = await _context.Emails.FindAsync(id);
        if (email == null || email.IsDeleted)
            return NotFound();

        email.IsDeleted = true;
        _context.Emails.Update(email);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Email deleted successfully." });
    }

    [HttpGet]
    public IActionResult BulkCreate()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> BulkCreate(string EmailsRaw)
    {
        if (string.IsNullOrWhiteSpace(EmailsRaw))
        {
            ModelState.AddModelError("", "Please enter at least one email.");
            return View();
        }

        var emails = EmailsRaw
            .Split(new[] { '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Trim())
            .Where(e => !string.IsNullOrEmpty(e))
            .Distinct()
            .ToList();

        var invalidEmails = emails.Where(e => !new EmailAddressAttribute().IsValid(e)).ToList();

        if (invalidEmails.Any())
        {
            ModelState.AddModelError("", "Invalid emails: " + string.Join(", ", invalidEmails));
            return View();
        }

        // Create list of Email entities
        var emailEntities = emails.Select(email => new Email
        {
            EmailAddress = email,
            Class = "FinalYear",
            CreatedDate = DateTimeOffset.UtcNow,
            IsDeleted = false
        }).ToList();

        // Add all emails in one go using AddRange
        _context.Emails.AddRange(emailEntities);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"{emails.Count} emails added successfully.";
        return RedirectToAction("Index");
    }


}
