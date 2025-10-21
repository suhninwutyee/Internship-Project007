using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
//using ProjectManagementSystem.Data;
using ProjectManagementSystem.DBModels;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.ViewModels;
using System.Linq;

public class WelcomeController : Controller
{
    private readonly PMSDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public WelcomeController(PMSDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public IActionResult Index()
    {
        int studentCount = _context.Users.Count(u =>
            _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == _context.Roles.FirstOrDefault(r => r.Name == "Student").Id));

        int teacherCount = _context.Users.Count(u =>
            _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == _context.Roles.FirstOrDefault(r => r.Name == "Teacher").Id));

        var model = new HomeViewModel
        {
            ProjectCount = _context.Projects.Count(),
            CompanyCount = _context.Companies.Count(),
            StudentCount = studentCount,
            TeacherCount = teacherCount
        };

        return View(model);
    }
}
