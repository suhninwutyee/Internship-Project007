//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using ProjectManagementSystem.Data;
//using ProjectManagementSystem.Models;
//using ProjectManagementSystem.Services;
//using System;
//using System.Linq;
//using System.Threading.Tasks;

//namespace ProjectManagementSystem.Controllers
//{
//    public class StudentLoginController : Controller
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly IEmailService _emailService;

//        public StudentLoginController(ApplicationDbContext context, IEmailService emailService)
//        {
//            _context = context;
//            _emailService = emailService;
//        }

//        // STEP 1: Show Login Form
//        public IActionResult Login()
//        {
//            return View();
//        }

//        // STEP 2-4: Process Login, Send OTP, and Save OTP
//        [HttpPost]
//        public async Task<IActionResult> Login(StudentLoginViewModel model)
//        {
//            if (!ModelState.IsValid)
//                return View(model);

//            string rollNo = model.RollNumber?.Trim().ToLower();
//            string email = model.EmailAddress?.Trim().ToLower();

//            // Try find student with project group
//            var student = _context.Students
//                .Include(s => s.ProjectMembers)
//                .FirstOrDefault(s => s.Email.RollNumber.ToLower() == rollNo &&
//                                     s.Email.EmailAddress.ToLower() == email &&
//                                     !s.IsDeleted &&
//                                     s.ProjectMembers.Any(pm => !pm.IsDeleted));

//            if (student != null)
//            {
//                // Process OTP for student
//                return await ProcessOtpForUser(student.Email.RollNumber, student.Email.EmailAddress);
//            }
//            ///////

//            // Fallback: Try find email record (older logic)
//            var emailRecord = _context.Emails
//                .FirstOrDefault(e => e.RollNumber.Trim().ToLower() == rollNo &&
//                                     e.EmailAddress.Trim().ToLower() == email &&
//                                     !e.IsDeleted);
           
//            if (emailRecord == null)
//            {
//                ViewBag.Error = "Your Email Address Not Registered In The System, Please Contact Student Affairs or Phone-xxxxxxxx";
//                return View(model);
//            }

//            // Process OTP for emailRecord
//            return await ProcessOtpForUser(emailRecord.RollNumber, emailRecord.EmailAddress);
//        }

//        // Extract OTP generation logic to a reusable method
//        private async Task<IActionResult> ProcessOtpForUser(string rollNumber, string emailAddress)
//        {
//            TempData["RollNumber"] = rollNumber;
//            TempData["EmailAddress"] = emailAddress;

//            // Check cooldown: prevent generating OTP if recent unexpired OTP exists
//            var recentOtp = _context.OTPs
//                .Where(o => o.RollNumber == rollNumber && !o.IsUsed)
//                .OrderByDescending(o => o.SendTime)
//                .FirstOrDefault();

//            if (recentOtp != null && recentOtp.SendTime.AddMinutes(1) > DateTime.Now)
//            {
//                ViewBag.Error = $"An OTP was recently sent. Please wait {Math.Ceiling((recentOtp.SendTime.AddMinutes(1) - DateTime.Now).TotalSeconds)} seconds before requesting a new one.";
//                return View("Login");  // Return login view with error
//            }

//            string otpCode = new Random().Next(100000, 999999).ToString();

//            try
//            {
//                await _emailService.SendEmailAsync(emailAddress, "Your OTP Code", $"Your OTP is: {otpCode}");
//            }
//            catch (Exception ex)
//            {
//                ViewBag.Error = "Email could not be sent: " + ex.Message;
//                return View("Login");
//            }

//            var otpEntry = new OTP
//            {
//                RollNumber = rollNumber,
//                OTPCode = otpCode,
//                SendTime = DateTime.Now,
//                IsUsed = false
//            };

//            _context.OTPs.Add(otpEntry);
//            await _context.SaveChangesAsync();

//            TempData["RollNumber"] = rollNumber;
//            return RedirectToAction("VerifyOtp");
//        }

//        // STEP 5: Show OTP Input View (GET)
//        public IActionResult VerifyOtp()
//        {
//            var rollNo = TempData["RollNumber"]?.ToString();
//            if (string.IsNullOrEmpty(rollNo))
//            {
//                TempData["Error"] = "Session expired. Please login again.";
//                return RedirectToAction("Login");
//            }

//            // Preserve RollNumber for the POST back
//            TempData.Keep("RollNumber");

//            var viewModel = new VerifyOtpViewModel
//            {
//                RollNumber = rollNo
//            };

//            return View(viewModel);
//        }

//        // STEP 6: Verify OTP (POST)
//        [HttpPost]
//        public IActionResult VerifyOtp(VerifyOtpViewModel model)
//        {
//            if (!ModelState.IsValid)
//                return View(model);

//            var otp = _context.OTPs
//                .Where(o => o.RollNumber == model.RollNumber && !o.IsUsed)
//                .OrderByDescending(o => o.SendTime)
//                .FirstOrDefault();

//            if (otp != null &&
//                otp.OTPCode == model.OTPCode &&
//                otp.SendTime.AddMinutes(1) > DateTime.Now) // OTP valid for 1 minute
//            {
//                otp.IsUsed = true;
//                _context.SaveChanges();

//                HttpContext.Session.SetString("RollNumber", model.RollNumber);
//                // 🔽 Get the actual student
//                var student = _context.Students
//                    .FirstOrDefault(s => s.Email.RollNumber == model.RollNumber && !s.IsDeleted==false);

//                if (student != null)
//                {
//                    HttpContext.Session.SetString("StudentName", student.StudentName);
//                    HttpContext.Session.SetString("EmailAddress", student.Email.EmailAddress);
//                }
//                else
//                {
//                    // fallback to email record (if no Student found)
//                    var emailRecord = _context.Emails
//                        .FirstOrDefault(e => e.RollNumber == model.RollNumber && !e.IsDeleted);

//                    if (emailRecord != null)
//                    {
//                        HttpContext.Session.SetString("StudentName", "Student");
//                        HttpContext.Session.SetString("EmailAddress", emailRecord.EmailAddress);
//                    }
//                }
//                return RedirectToAction("ChooseRole");
//            }

//            ViewBag.Error = "Invalid or expired OTP.";
//            return View(model);
//        }

//        // Resend OTP action (called via AJAX)
//        [HttpPost]
//        public async Task<IActionResult> ResendOtp(string rollNumber)
//        {
//            if (string.IsNullOrWhiteSpace(rollNumber))
//                return Json(new { success = false, message = "Roll number required." });

//            var emailRecord = _context.Emails.FirstOrDefault(e => e.RollNumber == rollNumber && !e.IsDeleted);
//            if (emailRecord == null)
//                return Json(new { success = false, message = "Email not found." });

//            // Mark old unused OTPs as used
//            var oldOtps = _context.OTPs.Where(o => o.RollNumber == rollNumber && !o.IsUsed).ToList();
//            foreach (var otp in oldOtps)
//                otp.IsUsed = true;

//            await _context.SaveChangesAsync();

//            // Generate new OTP
//            var otpCode = new Random().Next(100000, 999999).ToString();

//            var newOtp = new OTP
//            {
//                RollNumber = rollNumber,
//                OTPCode = otpCode,
//                SendTime = DateTime.Now,
//                IsUsed = false
//            };

//            _context.OTPs.Add(newOtp);
//            await _context.SaveChangesAsync();

//            try
//            {
//                await _emailService.SendEmailAsync(
//                    emailRecord.EmailAddress,
//                    "Your New OTP Code",
//                    $"Your new OTP code is: {otpCode}");

//                return Json(new { success = true, message = "OTP resent successfully to your email." });
//            }
//            catch (Exception ex)
//            {
//                return Json(new { success = false, message = $"Failed to send OTP email: {ex.Message}" });
//            }
//        }

        
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult Logout()
//        {
//            HttpContext.Session.Clear(); // Clears RollNumber and other session values
//            TempData.Clear();            // Clears all TempData values (if needed)
//            TempData["Success"] = "You have been logged out successfully.";
//            return RedirectToAction("Login", "StudentLogin");
//        }

//        public IActionResult ChooseRole()
//        {
//            var rollNumber = HttpContext.Session.GetString("RollNumber");
//            if (string.IsNullOrEmpty(rollNumber))
//            {
//                TempData["Error"] = "Session expired. Please login again.";
//                return RedirectToAction("Login");
//            }

//            // Check if student is already a leader in any project
//            var isLeader = _context.ProjectMembers
//                .Any(pm => pm.Student.Email.RollNumber == rollNumber &&
//                          pm.Role == "Leader" &&
//                          !pm.IsDeleted);

//            // If already a leader, redirect to dashboard directly
//            if (isLeader)
//            {
//                return RedirectToAction("Dashboard", "Student");
//            }

//            return View();
//        }
//        [HttpPost]
//        public IActionResult ChooseRole(string role)
//        {
//            var rollNumber = HttpContext.Session.GetString("RollNumber");
//            if (string.IsNullOrEmpty(rollNumber))
//            {
//                TempData["Error"] = "Session expired. Please login again.";
//                return RedirectToAction("Login", "StudentLogin");
//            }

//            HttpContext.Session.SetString("UserRole", role);

//            if (role == "Leader")
//            {
//                // 1. Check if Student info exists
//                var student = _context.Students
//                    .Include(s => s.Email)
//                    .FirstOrDefault(s => s.Email.RollNumber == rollNumber && !s.IsDeleted);

//                if (student == null)
//                {
//                    TempData["NextAction"] = "CreateProject";
//                    return RedirectToAction("Create", "Student");
//                }

//                // Store Student_pkId in session for use in Dashboard
//                HttpContext.Session.SetInt32("Student_pkId", student.Student_pkId);

//                // 2. Check if Project exists
//                var hasProject = _context.ProjectMembers
//                    .Any(pm => pm.Student_pkId == student.Student_pkId && pm.Role == "Leader" && !pm.IsDeleted);

//                if (!hasProject)
//                {
//                    return RedirectToAction("Create", "Project");
//                }

//                return RedirectToAction("Dashboard", "Student");
//            }
//            else // Member role
//            {
//                // 3. Check if student is in any project (as member)
//                var student = _context.Students
//                    .Include(s => s.Email)
//                    .FirstOrDefault(s => s.Email.RollNumber == rollNumber && !s.IsDeleted);

//                if (student == null)
//                {
//                    TempData["Error"] = "Student information not found.";
//                    return RedirectToAction("Create", "Student");
//                }

//                // Store Student_pkId in session
//                HttpContext.Session.SetInt32("Student_pkId", student.Student_pkId);

//                var isInProject = _context.ProjectMembers
//                    .Any(pm => pm.Student_pkId == student.Student_pkId && !pm.IsDeleted);

//                if (isInProject)
//                {
//                    return RedirectToAction("Dashboard", "Student");
//                }

//                TempData["Info"] = "You have not been added to any project yet. Please contact your project leader.";
//                return RedirectToAction("Dashboard", "Student");
//            }
//        }
//    }
//}

using Microsoft.AspNetCore.Http;
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
        private readonly ILogger<StudentLoginController> _logger;

        public StudentLoginController(ApplicationDbContext context, IEmailService emailService, ILogger<StudentLoginController> logger)
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
                    !e.IsDeleted);

            if (emailRecord == null)
            {
                TempData["Error"] = "Credentials not found. Please contact Student Affairs.";
                return RedirectToAction("Login");
            }

            // Then find student with relaxed requirements
            var student = await _context.Students
                .Include(s => s.Email)
                .Include(s => s.ProjectMembers)
                .FirstOrDefaultAsync(s =>
                    s.Email != null &&
                    s.Email.RollNumber.ToLower() == rollNo &&
                    s.Email.EmailAddress.ToLower() == email &&
                    !s.IsDeleted);

            if (student == null)
            {
                // Allow login through just email record if student record missing
                return await ProcessOtpForUser(emailRecord.RollNumber, emailRecord.EmailAddress);
            }

            // If student exists, proceed with OTP
            return await ProcessOtpForUser(student.Email.RollNumber, student.Email.EmailAddress);
        }

        private async Task<IActionResult> ProcessOtpForUser(string rollNumber, string emailAddress)
        {
            // Clear previous TempData
            TempData.Remove("RollNumber");
            TempData.Remove("EmailAddress");

            var recentOtp = await _context.OTPs
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
                    $"Your verification code is: {otpCode}\nValid for 5 minutes.");

                // Invalidate previous OTPS
                var oldOtps = await _context.OTPs
                    .Where(o => o.RollNumber == rollNumber && !o.IsUsed)
                    .ToListAsync();

                foreach (var otp in oldOtps)
                {
                    otp.IsUsed = true;
                }

                var otpEntry = new OTP
                {
                    RollNumber = rollNumber,
                    OTPCode = otpCode,
                    SendTime = DateTime.Now,
                    IsUsed = false
                    //ExpiryTime = DateTime.Now.AddMinutes(5)
                };

                _context.OTPs.Add(otpEntry);
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

        //[HttpPost]
        //public async Task<IActionResult> Login(StudentLoginViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //        return View(model);

        //    string rollNo = model.RollNumber?.Trim().ToLower();
        //    string email = model.EmailAddress?.Trim().ToLower();

        //    var student = await _context.Students
        //        .Include(s => s.Email)
        //        .Include(s => s.ProjectMembers)
        //        .FirstOrDefaultAsync(s =>
        //            s.Email != null &&
        //            s.Email.RollNumber.ToLower() == rollNo &&
        //            s.Email.EmailAddress.ToLower() == email &&
        //            !s.IsDeleted &&
        //            s.ProjectMembers.Any(pm => !pm.IsDeleted));

        //    if (student != null && student.Email != null)
        //    {
        //        return await ProcessOtpForUser(student.Email.RollNumber, student.Email.EmailAddress);
        //    }

        //    var emailRecord = await _context.Emails
        //        .FirstOrDefaultAsync(e =>
        //            e.RollNumber.Trim().ToLower() == rollNo &&
        //            e.EmailAddress.Trim().ToLower() == email &&
        //            !e.IsDeleted);

        //    if (emailRecord == null)
        //    {
        //        ViewBag.Error = "Your Email Address is not registered in the system. Please contact Student Affairs or phone xxxxxxxx.";
        //        return View(model);
        //    }

        //    return await ProcessOtpForUser(emailRecord.RollNumber, emailRecord.EmailAddress);
        //}

        //private async Task<IActionResult> ProcessOtpForUser(string rollNumber, string emailAddress)
        //{
        //    TempData["RollNumber"] = rollNumber;
        //    TempData["EmailAddress"] = emailAddress;

        //    var recentOtp = _context.OTPs
        //        .Where(o => o.RollNumber == rollNumber && !o.IsUsed)
        //        .OrderByDescending(o => o.SendTime)
        //        .FirstOrDefault();

        //    if (recentOtp != null && recentOtp.SendTime.AddMinutes(1) > DateTime.Now)
        //    {
        //        ViewBag.Error = $"An OTP was recently sent. Please wait {Math.Ceiling((recentOtp.SendTime.AddMinutes(1) - DateTime.Now).TotalSeconds)} seconds.";
        //        return View("Login");
        //    }

        //    string otpCode = new Random().Next(100000, 999999).ToString();

        //    try
        //    {
        //        await _emailService.SendEmailAsync(emailAddress, "Your OTP Code", $"Your OTP is: {otpCode}");
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.Error = "Email could not be sent: " + ex.Message;
        //        return View("Login");
        //    }

        //    var otpEntry = new OTP
        //    {
        //        RollNumber = rollNumber,
        //        OTPCode = otpCode,
        //        SendTime = DateTime.Now,
        //        IsUsed = false
        //    };

        //    _context.OTPs.Add(otpEntry);
        //    await _context.SaveChangesAsync();

        //    TempData["RollNumber"] = rollNumber;
        //    return RedirectToAction("VerifyOtp");
        //}

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

            var otp = _context.OTPs
                .Where(o => o.RollNumber == model.RollNumber && !o.IsUsed)
                .OrderByDescending(o => o.SendTime)
                .FirstOrDefault();

            if (otp != null &&
                otp.OTPCode == model.OTPCode &&
                otp.SendTime.AddMinutes(1) > DateTime.Now)
            {
                Console.WriteLine("here success otp.......................................");
                otp.IsUsed = true;
                _context.SaveChanges();

                HttpContext.Session.SetString("RollNumber", model.RollNumber);

                var student = _context.Students
                    .Include(s => s.Email)
                    .FirstOrDefault(s => s.Email.RollNumber == model.RollNumber && !s.IsDeleted);
                Console.WriteLine("here student nul?........................" + (student == null));
                if (student != null)
                {
                    Console.WriteLine("here student not null.........................");
                    HttpContext.Session.SetString("StudentName", student.StudentName);
                    HttpContext.Session.SetString("EmailAddress", student.Email?.EmailAddress ?? "");
                }
                else
                {
                    Console.WriteLine("here student null.........................");

                    var emailRecord = _context.Emails
                        .FirstOrDefault(e => e.RollNumber == model.RollNumber && !e.IsDeleted);

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
        public async Task<IActionResult> ResendOtp(string rollNumber)
        {
            if (string.IsNullOrWhiteSpace(rollNumber))
                return Json(new { success = false, message = "Roll number required." });

            var emailRecord = await _context.Emails
                .FirstOrDefaultAsync(e => e.RollNumber == rollNumber && !e.IsDeleted);

            if (emailRecord == null)
                return Json(new { success = false, message = "Email not found." });

            var oldOtps = _context.OTPs
                .Where(o => o.RollNumber == rollNumber && !o.IsUsed).ToList();

            foreach (var otp in oldOtps)
                otp.IsUsed = true;

            await _context.SaveChangesAsync();

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
                .Any(pm => pm.Student.Email.RollNumber == rollNumber &&
                           pm.Role == "Leader" &&
                           !pm.IsDeleted);

            Console.WriteLine("here isLeader ........................." + isLeader);

            if (isLeader)
            {
                var student = _context.Students
                .Include(s => s.Email)
                .FirstOrDefault(s => s.Email.RollNumber == rollNumber && !s.IsDeleted);
                if (student == null)
                {
                    TempData["NextAction"] = "CreateProject";
                    return RedirectToAction("Create", "Student");
                }
                HttpContext.Session.SetInt32("Student_pkId", student.Student_pkId);

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
                .Include(s => s.Email)
                .FirstOrDefault(s => s.Email.RollNumber == rollNumber && !s.IsDeleted);

            if (role == "Leader")
            {
                if (student == null)
                {
                    TempData["NextAction"] = "CreateProject";
                    return RedirectToAction("Create", "Student");
                }

                HttpContext.Session.SetInt32("Student_pkId", student.Student_pkId);

                var hasProject = _context.ProjectMembers
                    .Any(pm => pm.Student_pkId == student.Student_pkId &&
                               pm.Role == "Leader" &&
                               !pm.IsDeleted);

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

                HttpContext.Session.SetInt32("Student_pkId", student.Student_pkId);

                var isInProject = _context.ProjectMembers
                    .Any(pm => pm.Student_pkId == student.Student_pkId && !pm.IsDeleted);

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
