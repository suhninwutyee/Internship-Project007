using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementSystem.Controllers
{
    public class EmailController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmailController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string selectedYear, int page = 1)
        {
            int pageSize = 15;

            var query = _context.Emails
                 .Include(e => e.AcademicYear) // Include first
        .Where(e => !e.IsDeleted); // Include the AcademicYear navigation property

            if (!string.IsNullOrEmpty(selectedYear))
            {
                query = query.Where(e => e.AcademicYear.YearRange == selectedYear);
            }
            else
            {
                // Don't load anything if no year selected
                ViewBag.Emails = new List<Email>();
                ViewBag.AcademicYears = await GetAcademicYearsAsync();
                ViewBag.SelectedYear = null;
                ViewBag.TotalPages = 0;
                ViewBag.CurrentPage = 1;
                ViewBag.StartRowNumber = 1;
                return View(new List<Email>());
            }

            var totalEmails = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalEmails / (double)pageSize);
            var emails = await query
                .OrderBy(e => e.Email_PkId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.AcademicYears = await GetAcademicYearsAsync();
            ViewBag.SelectedYear = selectedYear;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.StartRowNumber = (page - 1) * pageSize + 1;

            return View(emails);
        }

        private async Task<List<AcademicYear>> GetAcademicYearsAsync()
        {
            return await _context.AcademicYears
                .OrderByDescending(y => y.YearRange)
                .ToListAsync();
        }

        // GET: Email/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.AcademicYears = await GetAcademicYearsAsync();
            return View();
        }

        // POST: Email/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmailAddress,RollNumber,AcademicYear_pkId")] Email email)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    email.Class = "Final Year";
                    _context.Add(email);
                    await _context.SaveChangesAsync();

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new
                        {
                            success = true,
                            message = "Email created successfully!",
                            redirectUrl = Url.Action("Index", new { selectedYear = _context.AcademicYears.Find(email.AcademicYear_pkId)?.YearRange })
                        });
                    }

                    TempData["SuccessMessage"] = "Email created successfully!";
                    return RedirectToAction("Index", new { selectedYear = _context.AcademicYears.Find(email.AcademicYear_pkId)?.YearRange });
                }
                catch (Exception ex)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Error creating email: " + ex.Message
                        });
                    }

                    ModelState.AddModelError("", "Error creating email: " + ex.Message);
                }
            }

            // Handle errors for AJAX
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return Json(new
                {
                    success = false,
                    errors,
                    message = "Please fix the validation errors"
                });
            }

            ViewBag.AcademicYears = await GetAcademicYearsAsync();
            return View(email);
        }


        // GET: Email/UploadBulk
        public async Task<IActionResult> UploadBulk()
        {
            ViewBag.AcademicYears = await GetAcademicYearsAsync();
            return View();
        }

        // POST: Email/UploadBulk
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadBulk(IFormFile file, int academicYear_pkId)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please upload a CSV file.";
                ViewBag.AcademicYears = await GetAcademicYearsAsync();
                return View();
            }

            var academicYear = await _context.AcademicYears.FindAsync(academicYear_pkId);
            if (academicYear == null)
            {
                TempData["Error"] = "Please select a valid academic year.";
                ViewBag.AcademicYears = await GetAcademicYearsAsync();
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

                    if (parts.Length < 3) continue; // Ensure StudentName, Email, RollNumber

                    var emailAddress = parts[0].Trim();
                    var rollNumber = parts[1].Trim();

                    if (string.IsNullOrWhiteSpace(emailAddress) ||
                        string.IsNullOrWhiteSpace(rollNumber))
                        continue;

                    emails.Add(new Email
                    {
                      
                        EmailAddress = emailAddress,
                        RollNumber = rollNumber,
                        Class = "Final Year",
                        AcademicYear_pkId = academicYear_pkId,
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

            ViewBag.AcademicYears = await GetAcademicYearsAsync();
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> EditInline(int id, [FromBody] Email email)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingEmail = await _context.Emails.FindAsync(id);
                    if (existingEmail == null)
                    {
                        return Json(new { success = false, message = "Email not found" });
                    }

                    existingEmail.EmailAddress = email.EmailAddress;
                    existingEmail.RollNumber = email.RollNumber;
                    existingEmail.Class = email.Class;

                    _context.Update(existingEmail);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true });
                }

                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return Json(new { success = false, errors });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInline(int id)
        {
            try
            {
                var email = await _context.Emails.FindAsync(id);
                if (email == null)
                {
                    return Json(new { success = false, message = "Email not found" });
                }

                // Soft delete approach (recommended)
                email.IsDeleted = true;
                await _context.SaveChangesAsync();

                // OR for hard delete:
                // _context.Emails.Remove(email);
                // await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Delete failed: " + ex.Message
                });
            }

        }
    }
}