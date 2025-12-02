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

            // Base query: Emails joined with AcademicYears
            var query = from e in _context.Emails
                        join a in _context.AcademicYears on e.AcademicYearPkId equals a.AcademicYearPkId
                        where e.IsDeleted == false
                        select new { Email = e, Year = a.YearRange };

            // Filter by selected year
            if (!string.IsNullOrEmpty(selectedYear))
            {
                query = query.Where(x => x.Year == selectedYear);
            }
            else
            {
                // If no year selected, show empty
                ViewBag.Emails = new List<DBModels.Email>();
                ViewBag.AcademicYears = await _context.AcademicYears
                                                      .OrderByDescending(y => y.YearRange)
                                                      .ToListAsync();
                ViewBag.SelectedYear = null;
                ViewBag.TotalPages = 0;
                ViewBag.CurrentPage = 1;
                ViewBag.StartRowNumber = 1;
                return View(new List<DBModels.Email>());
            }

            // Count total items for pagination
            var totalEmails = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalEmails / (double)pageSize);

            // Fetch current page items
            var emails = await query
                                .OrderBy(x => x.Email.EmailPkId)
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .Select(x => x.Email) // select tracked Email entities
                                .ToListAsync();

            // Pass data to View
            ViewBag.AcademicYears = await _context.AcademicYears
                                                  .OrderByDescending(y => y.YearRange)
                                                  .ToListAsync();
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
        // GET: Email/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.AcademicYears = await GetAcademicYearsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmailAddress,RollNumber,AcademicYearPkId")] DBModels.Email email)
        {
            // Always check model state first
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    // Return JSON for AJAX requests
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                    return Json(new
                    {
                        success = false,
                        errors,
                        message = "Validation failed"
                    });
                }

                // Normal post fallback
                ViewBag.AcademicYears = await GetAcademicYearsAsync();
                return View(email);
            }

            try
            {
                // Fill required fields
                email.Class = "Final Year";
                email.IsDeleted = false;
                email.CreatedDate = DateTimeOffset.Now;

                _context.Emails.Add(email);
                await _context.SaveChangesAsync();

                var yearRange = (await _context.AcademicYears.FindAsync(email.AcademicYearPkId))?.YearRange;

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = true,
                        message = "Email created successfully!",
                        redirectUrl = Url.Action("Index", new { selectedYear = yearRange })
                    });
                }

                TempData["SuccessMessage"] = "Email created successfully!";
                return RedirectToAction("Index", new { selectedYear = yearRange });
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
                ViewBag.AcademicYears = await GetAcademicYearsAsync();
                return View(email);
            }
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
        public async Task<IActionResult> UploadBulk(IFormFile file, int AcademicYearPkId)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please upload a CSV file.";
                ViewBag.AcademicYears = await GetAcademicYearsAsync();
                return View();
            }

            var academicYear = await _context.AcademicYears.FindAsync(AcademicYearPkId);
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
                        AcademicYearPkId = AcademicYearPkId,
                        CreatedDate = DateTime.Now,
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
        public async Task<IActionResult> EditInline(int id, [FromBody] DBModels.Email model)
        {
            if (model == null)
                return BadRequest(new { success = false, message = "Invalid data" });

            try
            {
                var existingEmail = await _context.Emails.FindAsync(id);
                if (existingEmail == null)
                    return NotFound(new { success = false, message = "Email not found" });

                if ((bool)existingEmail.IsDeleted)
                    return BadRequest(new { success = false, message = "Cannot edit deleted email" });

                // ✅ NEW: Check if no fields changed
                bool noChanges =
                    existingEmail.EmailAddress == model.EmailAddress &&
                    existingEmail.RollNumber == model.RollNumber &&
                    existingEmail.Class == model.Class;

                if (noChanges)
                {
                    return Json(new { success = false, message = "No changes detected" });
                }

                // Update fields
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
        public async Task<IActionResult> DeleteInline([FromBody] int id)
        {
            var email = await _context.Emails.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.EmailPkId == id);
            if (email == null)
                return Json(new { success = false, message = "Email not found" });

            if ((bool)email.IsDeleted)
                return Json(new { success = false, message = "Already deleted" });

            email.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }


    }
}