using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//using ProjectManagementSystem.Data;
using ProjectManagementSystem.DBModels;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementSystem.Controllers
{
    public class StudentLoginController : Controller
    {
        private readonly PMSDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<StudentLoginController> _logger;

        public StudentLoginController(PMSDbContext context, IEmailService emailService, ILogger<StudentLoginController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(StudentLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill all required fields correctly.";
                return View(model);
            }

            string rollNo = model.RollNumber?.Trim().ToLower();
            string email = model.EmailAddress?.Trim().ToLower();

            // First check if credentials exist at all
            var emailRecord = await _context.Emails
                .FirstOrDefaultAsync(e =>
                    e.RollNumber.Trim().ToLower() == rollNo &&
                    e.EmailAddress.Trim().ToLower() == email &&
                    e.IsDeleted == false);

            if (emailRecord == null)
            {
                TempData["Error"] = "Credentials not found. Please contact Student Affairs.";
                return RedirectToAction("Login");
            }

            // Then find student with relaxed requirements
            var student = await _context.Students
                .Include(s => s.EmailPk)
                .Include(s => s.ProjectMembers)
                .FirstOrDefaultAsync(s =>
                    s.EmailPk != null &&
                    s.EmailPk.RollNumber.ToLower() == rollNo &&
                    s.EmailPk.EmailAddress.ToLower() == email &&
                    s.IsDeleted == false);

            if (student == null)
            {
                // Allow login through just email record if student record missing
                return await ProcessOtpForUser(emailRecord.RollNumber, emailRecord.EmailAddress);
            }

            // If student exists, proceed with OTP
            return await ProcessOtpForUser(student.EmailPk .RollNumber, student.EmailPk.EmailAddress);
        }

        private async Task<IActionResult> ProcessOtpForUser(string rollNumber, string emailAddress)
        {
            // Clear previous TempData
            TempData.Remove("RollNumber");
            TempData.Remove("EmailAddress");

            var recentOtp = await _context.Otps
                .Where(o => o.RollNumber == rollNumber && !o.IsUsed)
                .OrderByDescending(o => o.SendTime)
                .FirstOrDefaultAsync();

            if (recentOtp != null && recentOtp.SendTime.AddSeconds(30) > DateTime.Now)
            {
                TempData["Error"] = $"Please wait {Math.Ceiling((recentOtp.SendTime.AddSeconds(30) - DateTime.Now).TotalSeconds)} seconds before requesting new OTP.";
                return RedirectToAction("Login");
            }

            string otpCode = new Random().Next(100000, 999999).ToString();

            try
            {
                await _emailService.SendEmailAsync(
                    emailAddress,
                    "Your Login OTP Code",
                    $"Your verification code is: {otpCode}");

                // Invalidate previous OTPS
                var oldOtps = await _context.Otps
                    .Where(o => o.RollNumber == rollNumber && !o.IsUsed)
                    .ToListAsync();

                foreach (var otp in oldOtps)
                {
                    otp.IsUsed = true;
                }

                var otpEntry = new Otp
                {
                    RollNumber = rollNumber,
                    Otpcode = otpCode,
                    SendTime = DateTime.Now,
                    IsUsed = false
                    //ExpiryTime = DateTime.Now.AddMinutes(5)
                };

                _context.Otps.Add(otpEntry);
                await _context.SaveChangesAsync();

                TempData["RollNumber"] = rollNumber;
                TempData["EmailAddress"] = emailAddress;
                return RedirectToAction("VerifyOtp");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OTP email failed to send");
                TempData["Error"] = "Failed to send OTP. Please try again later.";
                return RedirectToAction("Login");
            }
        }

       

        public IActionResult VerifyOtp()
        {
            var rollNo = TempData["RollNumber"]?.ToString();
            Console.WriteLine("roll no...................................." + rollNo);
            if (string.IsNullOrEmpty(rollNo))
            {
                Console.WriteLine("here roll  null....................................");
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login");
            }
            Console.WriteLine("here roll no null....................................");

            TempData.Keep("RollNumber");

            var viewModel = new VerifyOtpViewModel
            {
                RollNumber = rollNo
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult VerifyOtp(VerifyOtpViewModel model)
        {

            Console.WriteLine("her everify otp post......................................");

            var otp = _context.Otps
                .Where(o => o.RollNumber == model.RollNumber && !o.IsUsed)
                .OrderByDescending(o => o.SendTime)
                .FirstOrDefault();

            if (otp != null &&
                otp.Otpcode == model.OTPCode &&
                otp.SendTime.AddMinutes(1) > DateTime.Now)
            {
                Console.WriteLine("here success otp.......................................");
                otp.IsUsed = true;
                _context.SaveChanges();

                HttpContext.Session.SetString("RollNumber", model.RollNumber);

                var student = _context.Students
                    .Include(s => s.EmailPk)
                    .FirstOrDefault(s => s.EmailPk.RollNumber == model.RollNumber && s.IsDeleted == false);
                Console.WriteLine("here student nul?........................" + (student == null));
                if (student != null)
                {
                    Console.WriteLine("here student not null.........................");
                    HttpContext.Session.SetString("StudentName", student.StudentName);
                    HttpContext.Session.SetString("EmailAddress", student.EmailPk?.EmailAddress ?? "");
                }
                else
                {
                    Console.WriteLine("here student null.........................");

                    var emailRecord = _context.Emails
                        .FirstOrDefault(e => e.RollNumber == model.RollNumber && e.IsDeleted == false);

                    if (emailRecord != null)
                    {
                        HttpContext.Session.SetString("StudentName", "Student");
                        HttpContext.Session.SetString("EmailAddress", emailRecord.EmailAddress);
                    }
                }
                Console.WriteLine("roll number................................." + HttpContext.Session.GetString("RollNumber"));

                return RedirectToAction("ChooseRole");
            }

            ViewBag.Error = "Invalid or expired OTP.";
            return View(model);
        }

    

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOtp(string rollNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rollNumber))
                {
                    return Json(new { success = false, message = "Roll number is required." });
                }

                // Check if OTP was recently sent
                var recentOtp = await _context.Otps
                    .Where(o => o.RollNumber == rollNumber && !o.IsUsed)
                    .OrderByDescending(o => o.SendTime)
                    .FirstOrDefaultAsync();

                if (recentOtp != null && recentOtp.SendTime.AddSeconds(30) > DateTime.Now)
                {
                    var secondsLeft = (int)(recentOtp.SendTime.AddSeconds(30) - DateTime.Now).TotalSeconds;
                    return Json(new
                    {
                        success = false,
                        message = $"Please wait {secondsLeft} seconds before requesting a new OTP."
                    });
                }

                var emailRecord = await _context.Emails
                    .FirstOrDefaultAsync(e => e.RollNumber == rollNumber && e.IsDeleted == false);

                if (emailRecord == null)
                {
                    return Json(new { success = false, message = "Email not found. Please contact support." });
                }

                // Invalidate previous OTPs
                var oldOtps = await _context.Otps
                    .Where(o => o.RollNumber == rollNumber && !o.IsUsed)
                    .ToListAsync();

                foreach (var otp in oldOtps)
                {
                    otp.IsUsed = true;
                }

                // Generate new OTP
                string otpCode = new Random().Next(100000, 999999).ToString();

                var newOtp = new Otp
                {
                    RollNumber = rollNumber,
                    Otpcode = otpCode,
                    SendTime = DateTime.Now,
                    IsUsed = false
                };

                _context.Otps.Add(newOtp);
                await _context.SaveChangesAsync();

                // Send email
                await _emailService.SendEmailAsync(
                    emailRecord.EmailAddress,
                    "Your New Verification Code",
                    $"Your new verification code is: {otpCode}\n\nThis code will expire in 5 minutes.");

                _logger.LogInformation($"New OTP sent to {emailRecord.EmailAddress}");

                return Json(new
                {
                    success = true,
                    message = "A new OTP has been sent to your email address."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending OTP");
                return Json(new
                {
                    success = false,
                    message = "An error occurred while resending OTP. Please try again."
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData.Clear();
            TempData["Success"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }

        public IActionResult ChooseRole()
        {
            var rollNumber = HttpContext.Session.GetString("RollNumber");
            Console.WriteLine("roll number................................." + rollNumber);
            if (string.IsNullOrEmpty(rollNumber))
            {
                Console.WriteLine("here roll no null.........................");

                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login");
            }

            Console.WriteLine("here roll no not null.........................");

            var isLeader = _context.ProjectMembers
                .Any(pm => pm.StudentPk.EmailPk.RollNumber == rollNumber &&
                           pm.Role == "Leader" &&
                           pm.IsDeleted == false);

            Console.WriteLine("here isLeader ........................." + isLeader);

            if (isLeader)
            {
                var student = _context.Students
                .Include(s => s.EmailPk)
                .FirstOrDefault(s => s.EmailPk.RollNumber == rollNumber && s.IsDeleted == false);
                if (student == null)
                {
                    TempData["NextAction"] = "CreateProject";
                    return RedirectToAction("Create", "Student");
                }
                HttpContext.Session.SetInt32("Student_pkId", student.StudentPkId);

                Console.WriteLine("here is leader...............................");
                return RedirectToAction("Dashboard", "Student");
            }

            Console.WriteLine("here not leader...............................");

            return View();
        }

        [HttpPost]
        public IActionResult ChooseRole(string role)
        {
            var rollNumber = HttpContext.Session.GetString("RollNumber");
            Console.WriteLine("roll number................................." + rollNumber);

            if (string.IsNullOrEmpty(rollNumber))
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login");
            }

            HttpContext.Session.SetString("UserRole", role);

            var student = _context.Students
                .Include(s => s.EmailPk)
                .FirstOrDefault(s => s.EmailPk.RollNumber == rollNumber && s.IsDeleted == false);

            if (role == "Leader")
            {
                if (student == null)
                {
                    TempData["NextAction"] = "CreateProject";
                    return RedirectToAction("Create", "Student");
                }

                HttpContext.Session.SetInt32("Student_pkId", student.StudentPkId);

                var hasProject = _context.ProjectMembers
                    .Any(pm => pm.StudentPkId == student.StudentPkId &&
                               pm.Role == "Leader" &&
                               pm.IsDeleted == false);

                if (!hasProject)
                {
                    return RedirectToAction("Create", "Project");
                }

                return RedirectToAction("Dashboard", "Student");
            }
            else // Member
            {
                if (student == null)
                {
                    TempData["Error"] = "Student information not found.";
                    return RedirectToAction("Create", "Student");
                }

                HttpContext.Session.SetInt32("Student_pkId", student.StudentPkId);

                var isInProject = _context.ProjectMembers
                    .Any(pm => pm.StudentPkId == student.StudentPkId && pm.IsDeleted == false);

                if (isInProject)
                {
                    return RedirectToAction("Dashboard", "Student");
                }

                TempData["Info"] = "You have not been added to any project yet. Please contact your project leader.";
                return RedirectToAction("Dashboard", "Student");
            }
        }
    }
}
