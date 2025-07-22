using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProjectManagementSystem.Controllers
{
    public class MemberController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MemberController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Member
        public async Task<IActionResult> Index()
        {
            var members = await _context.ProjectMembers
                .Include(pm => pm.Student)
                .Include(pm => pm.Project)
                .Where(pm => !pm.IsDeleted)
                .ToListAsync();
            return View(members);
        }
        // GET: Member/Create
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProjectMember model)
        {
            if (ModelState.IsValid)
            {
                _context.ProjectMembers.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            LoadDropdowns();
            return View(model);
        }

        public IActionResult Edit(int id)
        {
            var projectMember = _context.ProjectMembers.FirstOrDefault(pm => pm.ProjectMember_pkId == id);
            if (projectMember == null)
            {
                return NotFound();
            }

            LoadDropdowns();
            return View(projectMember);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ProjectMember model)
        {
            if (ModelState.IsValid)
            {
                var existing = _context.ProjectMembers
                    .FirstOrDefault(pm => pm.ProjectMember_pkId == model.ProjectMember_pkId);

                if (existing == null)
                    return NotFound();

                // Only update the fields you allow to change
                existing.Role = model.Role;
                existing.Student_pkId = model.Student_pkId;
                existing.Project_pkId = model.Project_pkId;

                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            LoadDropdowns(); // Repopulate ViewBag
            return View(model);
        }
        private void LoadDropdowns()
        {
            //var students = _context.Students.ToList() ?? new List<Student>();
            //var projects = _context.Projects.ToList() ?? new List<Project>();

            // Get RollNumber of logged-in student from session
            var rollNumber = HttpContext.Session.GetString("RollNumber");

            var students = _context.Students
                .Where(s => s.RollNumber == rollNumber && !s.IsDeleted)
                .ToList();

            var projects = _context.Projects.ToList() ?? new List<Project>();

            ViewBag.Students = new SelectList(students, "Student_pkId", "StudentName");
            ViewBag.Projects = new SelectList(projects, "Project_pkId", "ProjectName");
        }

        // GET: Member/Remove/5
        public IActionResult Remove(int id)
        {
            var member = _context.ProjectMembers
                .Include(pm => pm.Student)
                .Include(pm => pm.Project)
                .FirstOrDefault(pm => pm.ProjectMember_pkId == id);

            if (member == null)
            {
                return NotFound();
            }

            return View(member); // Confirm delete page
        }

        // POST: Member/RemoveConfirmed/5
        [HttpPost, ActionName("Remove")]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveConfirmed(int id)
        {
            var member = _context.ProjectMembers.FirstOrDefault(pm => pm.ProjectMember_pkId == id);

            if (member == null)
            {
                return NotFound();
            }

            member.IsDeleted = true;
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

    }
}
