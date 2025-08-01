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
                .Where(e => !e.IsDeleted);

            if (!string.IsNullOrEmpty(selectedYear))
            {
                query = query.Where(e => e.AcademicYear.YearRange == selectedYear);
            }
            else
            {
                // Don't load anything if no year selected
                ViewBag.Emails = new List<Email>();
                ViewBag.AcademicYears = GenerateAcademicYears();
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

            ViewBag.AcademicYears = GenerateAcademicYears();
            ViewBag.SelectedYear = selectedYear;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.StartRowNumber = (page - 1) * pageSize + 1;

            return View(emails);
        }

        private List<string> GenerateAcademicYears()
        {
            var years = new List<string>();
            for (int year = DateTime.Now.Year; year >= 2000; year--)
            {
                years.Add($"{year - 1}-{year}");
            }
            return years;
        }

        // GET: Email/Create
        public IActionResult Create()
        {
            ViewBag.AcademicYears = GenerateAcademicYears();
            return View();
        }

        // POST: Email/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmailAddress,RollNumber,AcademicYear")] Email email)
        {
            if (ModelState.IsValid)
            {
                email.Class = "Final Year";
                email.CreatedDate = DateTimeOffset.Now;
                _context.Add(email);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.AcademicYears = GenerateAcademicYears();
            return View(email);
        }

        // GET: Email/UploadBulk
        public IActionResult UploadBulk()
        {
            ViewBag.AcademicYears = GenerateAcademicYears();
            return View();
        }

        // POST: Email/UploadBulk
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadBulk(IFormFile file, string academicYear)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please upload a CSV file.";
                ViewBag.AcademicYears = GenerateAcademicYears();
                return View();
            }

            if (string.IsNullOrEmpty(academicYear))
            {
                TempData["Error"] = "Please select an academic year.";
                ViewBag.AcademicYears = GenerateAcademicYears();
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
                        AcademicYear_pkId = 1,
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

            ViewBag.AcademicYears = GenerateAcademicYears();
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditInline(int id, [FromForm] Email model)
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
                existingEmail.AcademicYear = model.AcademicYear;

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