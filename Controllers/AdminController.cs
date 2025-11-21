// Controllers/AdminController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//using ProjectManagementSystem.Data;
using ProjectManagementSystem.DBModels;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.Services.Interface;
using ProjectManagementSystem.ViewModels;
using System.Security.Claims;
using System.Threading.Tasks;


namespace ProjectManagementSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IActivityLogger _activityLogger;
        private readonly PMSDbContext _context;

        public AdminController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IActivityLogger activityLogger,
            PMSDbContext context)
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
                    // Add custom claims
                    var claims = new List<Claim>
                    {
                        new Claim("FullName", user.FullName),
                        new Claim("Initial", user.FullName.Substring(0, 1).ToUpper())
                    };
                    Console.WriteLine(claims);
                    await _userManager.AddClaimsAsync(user, claims);

                    await _activityLogger.LogActivityAsync(
                        user.Id,
                        "Login",
                        user.Email,
                        HttpContext);

                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return RedirectToAction("Dashboard", "Teacher");
                    }
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
            Console.WriteLine("Logout action called"); // Debug output

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                Console.WriteLine($"Logging out user: {user.UserName}"); // Debug output
                await _activityLogger.LogActivityAsync(
                    user.Id,
                    "Logout",
                    user.FullName,
                    HttpContext);
            }
            else
            {
                Console.WriteLine("No user found to log out"); // Debug output
            }

            await _signInManager.SignOutAsync();
            Console.WriteLine("SignOutAsync completed"); // Debug output

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

        [HttpGet("Admin/MyProfile")]
        [Authorize]
        public async Task<IActionResult> MyProfile(bool isEditMode = false)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ProjectManagementSystem.DBModels.MyProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                IsEditMode = isEditMode
            };

            return View(model);
        }

        [HttpPost("Admin/MyProfile")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MyProfile(MyProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                model.IsEditMode = true;
                return View(model);
            }

            if (user.FullName != model.FullName)
            {
                // Update full name
                user.FullName = model.FullName;
                await _userManager.UpdateAsync(user);

                // Update claims
                var existingClaims = await _userManager.GetClaimsAsync(user);
                var fullNameClaim = existingClaims.FirstOrDefault(c => c.Type == "FullName");
                var initialClaim = existingClaims.FirstOrDefault(c => c.Type == "Initial");

                if (fullNameClaim != null)
                {
                    await _userManager.RemoveClaimAsync(user, fullNameClaim);
                }
                if (initialClaim != null)
                {
                    await _userManager.RemoveClaimAsync(user, initialClaim);
                }

                await _userManager.AddClaimsAsync(user, new List<Claim>
        {
            new Claim("FullName", model.FullName),
            new Claim("Initial", model.FullName.Substring(0, 1).ToUpper())
        });

                // Refresh the sign-in to update claims
                await _signInManager.RefreshSignInAsync(user);
            }

            // Handle password change if needed
            if (!string.IsNullOrWhiteSpace(model.OldPassword) &&
                !string.IsNullOrWhiteSpace(model.NewPassword))
            {
                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    model.IsEditMode = true;
                    return View(model);
                }

                user.IsUsingDefaultPassword = false;
                await _userManager.UpdateAsync(user);
                await _signInManager.RefreshSignInAsync(user);
            }

            TempData["SuccessMessage"] = "Profile updated successfully";
            return RedirectToAction(nameof(MyProfile));
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
