using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementSystem.Controllers
{
    public class StudentLoginController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public StudentLoginController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // STEP 1: Show Login Form
        public IActionResult Login()
        {
            return View();
        }

        // STEP 2-4: Process Login, Send OTP, and Save OTP
        [HttpPost]
        public async Task<IActionResult> Login(StudentLoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string rollNo = model.RollNumber?.Trim().ToLower();
            string email = model.EmailAddress?.Trim().ToLower();

            // Try find student with project group
            var student = _context.Students
                .Include(s => s.ProjectMembers)
                .FirstOrDefault(s => s.RollNumber.ToLower() == rollNo &&
                                     s.Email.ToLower() == email &&
                                     !s.IsDeleted &&
                                     s.ProjectMembers.Any(pm => !pm.IsDeleted));

            if (student != null)
            {
                // Process OTP for student
                return await ProcessOtpForUser(student.RollNumber, student.Email);
            }
            ///////
            
            // Fallback: Try find email record (older logic)
            var emailRecord = _context.Emails
                .FirstOrDefault(e => e.RollNumber.Trim().ToLower() == rollNo &&
                                     e.EmailAddress.Trim().ToLower() == email &&
                                     !e.IsDeleted);
           
            if (emailRecord == null)
            {
                ViewBag.Error = "Your Email Address Not Registered In The System, Please Contact Student Affairs or Phone-xxxxxxxx";
                return View(model);
            }

            // Process OTP for emailRecord
            return await ProcessOtpForUser(emailRecord.RollNumber, emailRecord.EmailAddress);
        }

        // Extract OTP generation logic to a reusable method
        private async Task<IActionResult> ProcessOtpForUser(string rollNumber, string emailAddress)
        {
            TempData["RollNumber"] = rollNumber;
            TempData["EmailAddress"] = emailAddress;

            // Check cooldown: prevent generating OTP if recent unexpired OTP exists
            var recentOtp = _context.OTPs
                .Where(o => o.RollNumber == rollNumber && !o.IsUsed)
                .OrderByDescending(o => o.SendTime)
                .FirstOrDefault();

            if (recentOtp != null && recentOtp.SendTime.AddMinutes(1) > DateTime.Now)
            {
                ViewBag.Error = $"An OTP was recently sent. Please wait {Math.Ceiling((recentOtp.SendTime.AddMinutes(1) - DateTime.Now).TotalSeconds)} seconds before requesting a new one.";
                return View("Login");  // Return login view with error
            }

            string otpCode = new Random().Next(100000, 999999).ToString();

            try
            {
                await _emailService.SendEmailAsync(emailAddress, "Your OTP Code", $"Your OTP is: {otpCode}");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Email could not be sent: " + ex.Message;
                return View("Login");
            }

            var otpEntry = new OTP
            {
                RollNumber = rollNumber,
                OTPCode = otpCode,
                SendTime = DateTime.Now,
                IsUsed = false
            };

            _context.OTPs.Add(otpEntry);
            await _context.SaveChangesAsync();

            TempData["RollNumber"] = rollNumber;
            return RedirectToAction("VerifyOtp");
        }

        // STEP 5: Show OTP Input View (GET)
        public IActionResult VerifyOtp()
        {
            var rollNo = TempData["RollNumber"]?.ToString();
            if (string.IsNullOrEmpty(rollNo))
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login");
            }

            // Preserve RollNumber for the POST back
            TempData.Keep("RollNumber");

            var viewModel = new VerifyOtpViewModel
            {
                RollNumber = rollNo
            };

            return View(viewModel);
        }

        // STEP 6: Verify OTP (POST)
        [HttpPost]
        public IActionResult VerifyOtp(VerifyOtpViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var otp = _context.OTPs
                .Where(o => o.RollNumber == model.RollNumber && !o.IsUsed)
                .OrderByDescending(o => o.SendTime)
                .FirstOrDefault();

            if (otp != null &&
                otp.OTPCode == model.OTPCode &&
                otp.SendTime.AddMinutes(1) > DateTime.Now) // OTP valid for 1 minute
            {
                otp.IsUsed = true;
                _context.SaveChanges();

                HttpContext.Session.SetString("RollNumber", model.RollNumber);
                // 🔽 Get the actual student
                var student = _context.Students
                    .FirstOrDefault(s => s.RollNumber == model.RollNumber && !s.IsDeleted);

                if (student != null)
                {
                    HttpContext.Session.SetString("StudentName", student.StudentName);
                    HttpContext.Session.SetString("EmailAddress", student.Email);
                }
                else
                {
                    // fallback to email record (if no Student found)
                    var emailRecord = _context.Emails
                        .FirstOrDefault(e => e.RollNumber == model.RollNumber && !e.IsDeleted);

                    if (emailRecord != null)
                    {
                        HttpContext.Session.SetString("StudentName", "Student");
                        HttpContext.Session.SetString("EmailAddress", emailRecord.EmailAddress);
                    }
                }
                return RedirectToAction("Create", "Student");
            }

            ViewBag.Error = "Invalid or expired OTP.";
            return View(model);
        }

        // Resend OTP action (called via AJAX)
        [HttpPost]
        public async Task<IActionResult> ResendOtp(string rollNumber)
        {
            if (string.IsNullOrWhiteSpace(rollNumber))
                return Json(new { success = false, message = "Roll number required." });

            var emailRecord = _context.Emails.FirstOrDefault(e => e.RollNumber == rollNumber && !e.IsDeleted);
            if (emailRecord == null)
                return Json(new { success = false, message = "Email not found." });

            // Mark old unused OTPs as used
            var oldOtps = _context.OTPs.Where(o => o.RollNumber == rollNumber && !o.IsUsed).ToList();
            foreach (var otp in oldOtps)
                otp.IsUsed = true;

            await _context.SaveChangesAsync();

            // Generate new OTP
            var otpCode = new Random().Next(100000, 999999).ToString();

            var newOtp = new OTP
            {
                RollNumber = rollNumber,
                OTPCode = otpCode,
                SendTime = DateTime.Now,
                IsUsed = false
            };

            _context.OTPs.Add(newOtp);
            await _context.SaveChangesAsync();

            try
            {
                await _emailService.SendEmailAsync(
                    emailRecord.EmailAddress,
                    "Your New OTP Code",
                    $"Your new OTP code is: {otpCode}");

                return Json(new { success = true, message = "OTP resent successfully to your email." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Failed to send OTP email: {ex.Message}" });
            }
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Clears RollNumber and other session values
            TempData.Clear();            // Clears all TempData values (if needed)
            TempData["Success"] = "You have been logged out successfully.";
            return RedirectToAction("Login", "StudentLogin");
        }
    }
}

