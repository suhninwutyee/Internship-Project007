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

        // POST: Member/Create
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

            // Repopulate dropdowns on validation failure
            LoadDropdowns();
            return View(model);
        }

        private void LoadDropdowns()
        {
            var students = _context.Students.ToList() ?? new List<Student>();
            var projects = _context.Projects.ToList() ?? new List<Project>();
            Console.WriteLine("s list..........................." + JsonSerializer.Serialize(students));
            Console.WriteLine("plist..........................." + JsonSerializer.Serialize(projects));
            ViewBag.Students = new SelectList(students, "Student_pkId", "RollNumber");
            ViewBag.Projects = new SelectList(projects, "Project_pkId", "ProjectName");
        }
        // GET: Member/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = _context.ProjectMembers
                                 .Include(pm => pm.Student)
                                 .Include(pm => pm.Project)
                                 .FirstOrDefault(pm => pm.ProjectMember_pkId == id);

            if (member == null)
            {
                return NotFound();
            }

            LoadDropdowns(); // Repopulate Student and Project dropdowns
            return View(member);
        }

        // POST: Member/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ProjectMember model)
        {
            if (id != model.ProjectMember_pkId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.ProjectMembers.Update(model);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.ProjectMembers.Any(e => e.ProjectMember_pkId == model.ProjectMember_pkId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            LoadDropdowns(); // Repopulate dropdowns on validation error
            return View(model);
        }


        // GET: Member/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var member = await _context.ProjectMembers
                .Include(pm => pm.Student)
                .Include(pm => pm.Project)
                .FirstOrDefaultAsync(m => m.ProjectMember_pkId == id);

            if (member == null || member.IsDeleted)
                return NotFound();

            return View(member);
        }

        // POST: Member/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var member = await _context.ProjectMembers.FindAsync(id);
            if (member != null)
            {
                member.IsDeleted = true; // soft delete
                _context.Update(member);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectMemberExists(int id)
        {
            return _context.ProjectMembers.Any(e => e.ProjectMember_pkId == id && !e.IsDeleted);
        }
    }
}
