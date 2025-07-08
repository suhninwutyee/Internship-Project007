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

            // ✅ STEP 2: Check if Email exists for the given RollNumber
            var emailRecord = _context.Emails
                .FirstOrDefault(e => e.RollNumber.Trim().ToLower() == rollNo &&
                                     e.EmailAddress.Trim().ToLower() == email &&
                                     !e.IsDeleted);

            if (emailRecord == null)
            {
                ViewBag.Error = "Your Email Address Not Registered In The System, Please Contact Student Affairs or Phone-xxxxxxxx";
                return View(model);
            }
            // ✅ Save for later use (e.g., pre-fill Student Create form)
            TempData["RollNumber"] = emailRecord.RollNumber;
            TempData["EmailAddress"] = emailRecord.EmailAddress;

            // ✅ STEP 3: Generate OTP
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

            // ✅ STEP 4: Save OTP
            var otpEntry = new OTP
            {
                RollNumber = emailRecord.RollNumber,
                OTPCode = otpCode,
                SendTime = DateTime.Now
                // ❌ Removed Student_pkId since we don't use Student table
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
                ViewBag.Error = "Session expired. Please login again.";
                return RedirectToAction("Login");
            }

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

            var latestOtp = _context.OTPs
                .Where(o => o.RollNumber == model.RollNumber)
                .OrderByDescending(o => o.SendTime)
                .FirstOrDefault();

            if (latestOtp != null &&
                latestOtp.OTPCode == model.OTPCode &&
                latestOtp.SendTime.AddMinutes(5) > DateTime.Now)
            {
                // ✅ Redirect to Student creation form
                TempData["RollNumber"] = model.RollNumber;
                return RedirectToAction("Create", "Student");
            }

            ViewBag.Error = "Invalid or expired OTP.";
            return View(model);
        }

        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
