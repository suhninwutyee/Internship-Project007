// Controllers/AdminController.cs
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.Services.Interface;
using ProjectManagementSystem.ViewModels;


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

                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    await _activityLogger.LogActivityAsync(
                        user.Id,
                        "Login",
                        user.Email,
                        HttpContext);

                    return RedirectToAction("Dashboard", "Teacher");
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
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {


            var user = await _userManager.GetUserAsync(User);
            Console.WriteLine("user null?............................" + (user == null));

            if (user == null)
            {
                return RedirectToAction("Login", "Admin");
            }
            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            Console.WriteLine("changePasswordResult?............................" + changePasswordResult);

            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["StatusMessage"] = "Password changed successfully.";
            return RedirectToAction("ChangePassword");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MyProfile(bool isEditMode = false)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new MyProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                IsEditMode = isEditMode
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MyProfile(MyProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.IsEditMode = true;
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Update profile information
            if (user.FullName != model.FullName)
            {
                user.FullName = model.FullName;
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    model.IsEditMode = true;
                    return View(model);
                }
            }

            // Change password if provided
            if (!string.IsNullOrEmpty(model.OldPassword) &&
                !string.IsNullOrEmpty(model.NewPassword))
            {
                var changePasswordResult = await _userManager.ChangePasswordAsync(
                    user,
                    model.OldPassword,
                    model.NewPassword);

                if (!changePasswordResult.Succeeded)
                {
                    foreach (var error in changePasswordResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    model.IsEditMode = true;
                    return View(model);
                }

                await _signInManager.RefreshSignInAsync(user);
            }

            model.IsEditMode = false;
            TempData["SuccessMessage"] = "Profile updated successfully!";
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleEditMode()
        {
            return RedirectToAction(nameof(MyProfile), new { isEditMode = true });
        }


    }



}
