using Microsoft.AspNetCore.Mvc;
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

            var emailRecord = _context.Emails
                .FirstOrDefault(e => e.RollNumber.Trim().ToLower() == rollNo &&
                                     e.EmailAddress.Trim().ToLower() == email &&
                                     !e.IsDeleted);

            if (emailRecord == null)
            {
                ViewBag.Error = "Your Email Address Not Registered In The System, Please Contact Student Affairs or Phone-xxxxxxxx";
                return View(model);
            }

            TempData["RollNumber"] = emailRecord.RollNumber;
            TempData["EmailAddress"] = emailRecord.EmailAddress;

            // Check cooldown: prevent generating OTP if recent unexpired OTP exists
            var recentOtp = _context.OTPs
                .Where(o => o.RollNumber == emailRecord.RollNumber && !o.IsUsed)
                .OrderByDescending(o => o.SendTime)
                .FirstOrDefault();

            if (recentOtp != null && recentOtp.SendTime.AddMinutes(5) > DateTime.Now)
            {
                ViewBag.Error = $"An OTP was recently sent. Please wait {Math.Ceiling((recentOtp.SendTime.AddMinutes(5) - DateTime.Now).TotalSeconds)} seconds before requesting a new one.";
                return View(model);
            }

            // Generate new OTP
            string otpCode = new Random().Next(100000, 999999).ToString();

            try
            {
                await _emailService.SendEmailAsync(emailRecord.EmailAddress, "OTP Code", $"Your OTP is: {otpCode}");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Email could not be sent: " + ex.Message;
                return View(model);
            }

            var otpEntry = new OTP
            {
                RollNumber = emailRecord.RollNumber,
                OTPCode = otpCode,
                SendTime = DateTime.Now,
                IsUsed = false
            };

            _context.OTPs.Add(otpEntry);
            await _context.SaveChangesAsync();

            TempData["RollNumber"] = emailRecord.RollNumber;
            return RedirectToAction("VerifyOtp");
        }

        // STEP 5: Show OTP Input View
        public IActionResult VerifyOtp()
        {
            var rollNo = TempData["RollNumber"]?.ToString();
            if (string.IsNullOrEmpty(rollNo))
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login");
            }

            // Keep RollNumber in TempData for POST back
            TempData.Keep("RollNumber");

            var viewModel = new VerifyOtpViewModel
            {
                RollNumber = rollNo
            };

            return View(viewModel);
        }

        // STEP 6: Verify OTP
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
                otp.SendTime.AddMinutes(5) > DateTime.Now)
            {
                otp.IsUsed = true;
                _context.SaveChanges();
                
                TempData["RollNumber"] = model.RollNumber;
                return RedirectToAction("Dashboard", "Student");
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
    }
}

