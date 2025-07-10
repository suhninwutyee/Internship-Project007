// Controllers/AdminController.cs
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.Services.Interface;

namespace ProjectManagementSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IActivityLogger _activityLogger;
        private readonly ApplicationDbContext _context;

        public AdminController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IActivityLogger activityLogger,
            ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _activityLogger = activityLogger;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }

                // Update stored name if different
                if (user.FullName != model.Name)
                {
                    user.FullName = model.Name;
                    await _userManager.UpdateAsync(user);
                }

                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // Log using the EXACT name from the form
                    await _activityLogger.LogActivityAsync(
                        user.Id,
                        "Login",
                        model.Name,  // This is critical
                        HttpContext);

                    return RedirectToAction("Index", "Email");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                await _activityLogger.LogActivityAsync(
                    user.Id,
                    "Logout",
                    user.FullName,
                    HttpContext);
            }

            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActivityLogs()
        {
            var logs = await _context.AdminActivityLogs
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();

            return View(logs);
        }
    }
}