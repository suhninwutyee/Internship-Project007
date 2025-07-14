using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;

namespace ProjectManagementSystem.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Student/Create
        public IActionResult Create()
        {
            var nrcTypes = _context.NRCTypes.ToList();
            var townships = _context.NRCTownships.ToList();
            var regionCodes = townships.Select(t => t.RegionCode_M).Distinct().ToList();
            var departments = _context.StudentDepartments.OrderBy(d => d.DepartmentName).ToList();

            var viewModel = new NRCFormViewModel
            {
                Student = new Student(),
                NRCTypeList = nrcTypes,
                RegionCodeMList = regionCodes,
                TownshipList = townships,
                DepartmentList = departments,
                // ✅ Add this line to pass Project Members
                ProjectMembers = _context.ProjectMembers
                .Where(pm => !pm.IsDeleted)
                .ToList()
            };

            // ✅ Pre-fill Student info from TempData (from AddProjectMember)
            var rollNo = TempData["RollNumber"]?.ToString();
            var name = TempData["StudentName"]?.ToString();
            var email = TempData["EmailAddress"]?.ToString();
            var role = TempData["Role"]?.ToString();
            var projectId = TempData["Project_pkId"]?.ToString();

            if (!string.IsNullOrEmpty(rollNo)) viewModel.Student.RollNumber = rollNo;
            if (!string.IsNullOrEmpty(name)) viewModel.Student.StudentName = name;
            if (!string.IsNullOrEmpty(email)) viewModel.Student.Email = email;

            // ✅ Keep these values in TempData again (for POST method)
            TempData["Role"] = role;
            TempData["Project_pkId"] = projectId;

            // ✅ Pass project assignment status to view
            ViewBag.IsInProject = !string.IsNullOrEmpty(projectId);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NRCFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Student.CreatedDate = DateTime.Now;
                model.Student.IsDeleted = false;

                _context.Students.Add(model.Student);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Student created successfully!";
                return RedirectToAction("Dashboard", new { id = model.Student.Student_pkId });
            }

            // Reload dropdowns on validation failure
            model.NRCTypeList = _context.NRCTypes.ToList();
            model.RegionCodeMList = _context.NRCTownships.Select(t => t.RegionCode_M).Distinct().ToList();
            model.TownshipList = _context.NRCTownships.ToList();
            model.DepartmentList = _context.StudentDepartments.OrderBy(d => d.DepartmentName).ToList();

            return View(model);
        }

        // JSON: Get townships by RegionCode
        [HttpGet]
        public JsonResult GetTownshipsByRegion(string regionCode)
        {
            if (string.IsNullOrEmpty(regionCode))
            {
                return Json(new List<object>());
            }

            var townships = _context.NRCTownships
                .Where(t => t.RegionCode_M == regionCode)
                .Select(t => new
                {
                    nRC_pkId = t.NRC_pkId,
                    townshipCode_M = t.TownshipCode_M,
                    townshipCode_E = t.TownshipCode_E,
                    townshipName = t.TownshipName
                })
                .OrderBy(t => t.townshipCode_M)
                .ToList();

            return Json(townships);
        }
        // GET: Student/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var student = await _context.Students.FindAsync(id);
            if (student == null)
                return NotFound();

            var nrcTypes = _context.NRCTypes.ToList();
            var townships = _context.NRCTownships.ToList();
            var regionCodes = townships.Select(t => t.RegionCode_M).Distinct().ToList();
            var departments = _context.StudentDepartments.OrderBy(d => d.DepartmentName).ToList();

            var viewModel = new NRCFormViewModel
            {
                Student = student,
                NRCTypeList = nrcTypes,
                RegionCodeMList = regionCodes,
                TownshipList = townships,
                DepartmentList = departments
            };

            return View(viewModel);
        }

        // POST: Student/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NRCFormViewModel model)
        {
            if (id != model.Student.Student_pkId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model.Student);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Student updated successfully!";
                    return RedirectToAction("Dashboard", new { id = model.Student.Student_pkId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Students.Any(e => e.Student_pkId == id))
                        return NotFound();
                    else
                        throw;
                }
            }

            // Reload dropdowns if validation failed
            model.NRCTypeList = _context.NRCTypes.ToList();
            model.RegionCodeMList = _context.NRCTownships.Select(t => t.RegionCode_M).Distinct().ToList();
            model.TownshipList = _context.NRCTownships.ToList();
            model.DepartmentList = _context.StudentDepartments.OrderBy(d => d.DepartmentName).ToList();

            return View(model);
        }

        public async Task<IActionResult> Dashboard(int page = 1)
        {
            var students = await _context.Students
                .Include(s => s.StudentDepartment)
                .Include(s => s.ProjectMembers)
                .Where(s => !s.IsDeleted)
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();

            var projects = await _context.Projects
                .Include(p => p.ProjectType)
                .Include(p => p.Language)
                .Include(p => p.Framework)
                .Include(p => p.Company)
                .OrderByDescending(p => p.ProjectSubmittedDate)
                .ToPagedListAsync(page, 5); // You can change the page size as needed

            var viewModel = new StudentDashboardViewModel
            {
                Students = students,
                Projects = projects
            };

            return View(viewModel);
        }

    }
}
