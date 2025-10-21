using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.DBModels;
using ProjectManagementSystem.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementSystem.Controllers
{
    public class EmailController : Controller
    {
        private readonly PMSDbContext _context;

        public EmailController(PMSDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string selectedYear, int page = 1)
        {
            int pageSize = 15;

            var query = _context.Emails
                 .Include(e => e.AcademicYearPkId) // Include first
        .Where(e => e.IsDeleted == false); // Include the AcademicYear navigation property

            if (!string.IsNullOrEmpty(selectedYear))
            {
                //query = query.Where(e => e.AcademicYearPkId == selectedYear);
                query = from e in _context.Emails
                        join aca in _context.AcademicYears
                        on e.AcademicYearPkId equals aca.AcademicYearPkId
                        where aca.YearRange == selectedYear
                        select new DBModels.Email
                        {
                            EmailPkId = e.EmailPkId,
                            EmailAddress = e.EmailAddress,
                            RollNumber = e.RollNumber,
                            Class = e.Class,
                            IsDeleted = e.IsDeleted,
                            AcademicYearPkId = e.AcademicYearPkId
                        };
            }
            else
            {
                // Don't load anything if no year selected
                ViewBag.Emails = new List<DBModels.Email>();
                ViewBag.AcademicYears = await GetAcademicYearsAsync();
                ViewBag.SelectedYear = null;
                ViewBag.TotalPages = 0;
                ViewBag.CurrentPage = 1;
                ViewBag.StartRowNumber = 1;
                return View(new List<DBModels.Email>());
            }

            var totalEmails = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalEmails / (double)pageSize);
            var emails = await query
                .OrderBy(e => e.EmailPkId)
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

        private async Task<List<DBModels.AcademicYear>> GetAcademicYearsAsync()
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
        public async Task<IActionResult> Create([Bind("EmailAddress,RollNumber,AcademicYear_pkId")] DBModels.Email email)
        {
                try
                {
                    email.Class = "Final Year";
                    email.CreatedDate = DateTime.Now;
                    _context.Add(email);
                    await _context.SaveChangesAsync();

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new
                        {
                            success = true,
                            message = "Email created successfully!",
                            redirectUrl = Url.Action("Index", new { selectedYear = _context.AcademicYears.Find(email.AcademicYearPkId)?.YearRange })
                        });
                    }

                    TempData["SuccessMessage"] = "Email created successfully!";
                    return RedirectToAction("Index", new { selectedYear = _context.AcademicYears.Find(email.AcademicYearPkId)?.YearRange });
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

            var emails = new List<DBModels.Email>();
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

                    emails.Add(new DBModels.Email
                    {
                      
                        EmailAddress = emailAddress,
                        RollNumber = rollNumber,
                        Class = "Final Year",
                        AcademicYearPkId = academicYear_pkId,
                        CreatedDate= DateTime.Now,
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
        public async Task<IActionResult> EditInline(int id, [FromForm] DBModels.Email model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validation failed",
                    errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    )
                });
            }

            try
            {
                var existingEmail = await _context.Emails.FindAsync(id);
                if (existingEmail == null)
                {
                    return NotFound(new { success = false, message = "Email not found" });
                }

                    existingEmail.EmailAddress = model.EmailAddress;
                    existingEmail.RollNumber = model.RollNumber;
                    existingEmail.Class = model.Class;

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInline(int id)
        {
            var email = await _context.Emails.FindAsync(id);
            if (email == null)
            {
                return NotFound(new { success = false, message = "Email not found" });
            }

            email.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}