using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.Controllers
{
    public class ProjectSubmitController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectSubmitController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: List all projects with members
        public async Task<IActionResult> Index()
        {
            var projects = await _context.Projects
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.Student)
                .ToListAsync();

            var vmList = projects.Select(p => new ProjectSubmitViewModel
            {
                Project_pkId = p.Project_pkId,
                ProjectName = p.ProjectName,
                Description = p.Description,
                Members = p.ProjectMembers.Select(pm => new ProjectSubmitViewModel.ProjectMemberInfo
                {
                    Student_pkId = pm.Student.Student_pkId,
                    StudentName = pm.Student.StudentName,
                    Email = pm.Student.Email
                }).ToList()
            }).ToList();

            return View(vmList);
        }

        // GET: Create new project submission form
        public IActionResult Create()
        {
            ViewBag.Students = _context.Students.ToList(); // For dropdowns in form
            ViewBag.Projects = _context.Projects.ToList();

            return View();
        }

        // POST: Create new project submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int projectId, List<int> selectedStudentIds)
        {
            if (projectId == 0 || selectedStudentIds == null || !selectedStudentIds.Any())
            {
                ModelState.AddModelError("", "Project and members must be selected.");
                ViewBag.Students = _context.Students.ToList();
                ViewBag.Projects = _context.Projects.ToList();
                return View();
            }

            foreach (var studentId in selectedStudentIds)
            {
                var existing = await _context.ProjectMembers
                    .FirstOrDefaultAsync(pm => pm.Project_pkId == projectId && pm.Student_pkId == studentId);

                if (existing == null)
                {
                    _context.ProjectMembers.Add(new ProjectMember
                    {
                        Project_pkId = projectId,
                        Student_pkId = studentId
                    });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Optional: Edit or Delete actions for project submissions
    }
}
