using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AdminController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // GET: Admin/MyProfile
    [HttpGet]
    public async Task<IActionResult> MyProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var model = new MyProfileViewModel
        {
            FullName = user.FullName,
            Email = user.Email
        };

        return View(model);
    }

    // POST: Admin/MyProfile
    [HttpPost]
    public async Task<IActionResult> MyProfile(MyProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        // Update full name if changed
        if (user.FullName != model.FullName)
        {
            user.FullName = model.FullName;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                    ModelState.AddModelError("", error.Description);
                return View(model);
            }
        }

        // If password fields are filled, try to change password
        if (!string.IsNullOrEmpty(model.OldPassword) ||
            !string.IsNullOrEmpty(model.NewPassword) ||
            !string.IsNullOrEmpty(model.ConfirmPassword))
        {
            if (string.IsNullOrEmpty(model.OldPassword) ||
                string.IsNullOrEmpty(model.NewPassword) ||
                string.IsNullOrEmpty(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "To change password, all password fields must be filled.");
                return View(model);
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "The new password and confirmation password do not match.");
                return View(model);
            }

            var changePassResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePassResult.Succeeded)
            {
                foreach (var error in changePassResult.Errors)
                    ModelState.AddModelError("", error.Description);
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
        }

        TempData["StatusMessage"] = "Profile updated successfully.";
        return RedirectToAction(nameof(MyProfile));
    }
}

public class MyProfileViewModel
{
    [Required]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = "";

    [EmailAddress]
    public string Email { get; set; } = "";

    [DataType(DataType.Password)]
    [Display(Name = "Current Password")]
    public string? OldPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm New Password")]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation do not match.")]
    public string? ConfirmPassword { get; set; }
}
