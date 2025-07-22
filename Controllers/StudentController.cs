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
            var years = _context.AcademicYears.OrderByDescending(y => y.YearRange).ToList(); 
            
            var viewModel = new NRCFormViewModel
            {
                Student = new Student(),
                NRCTypeList = nrcTypes,
                RegionCodeMList = regionCodes,
                TownshipList = townships,
                DepartmentList = departments,
                AcademicYearList= years,
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
                
                HttpContext.Session.SetString("SuccessMessage", "Student created successfully!");
                return RedirectToAction("Dashboard", new { id = model.Student.Student_pkId });
            }

            model.NRCTypeList = _context.NRCTypes.ToList();
            model.RegionCodeMList = _context.NRCTownships.Select(t => t.RegionCode_M).Distinct().ToList();
            model.TownshipList = _context.NRCTownships.ToList();
            model.DepartmentList = _context.StudentDepartments.OrderBy(d => d.DepartmentName).ToList();
            model.AcademicYearList = _context.AcademicYears.OrderByDescending(a => a.YearRange).ToList();
           
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
        public async Task<IActionResult> Edit(int id)
        {
            var student = await _context.Students
                .Include(s => s.NRCTownship)
                .Include(s => s.NRCType)
                .Include(s => s.StudentDepartment)
                .Include(s => s.AcademicYear)
                .FirstOrDefaultAsync(s => s.Student_pkId == id);

            if (student == null)
            {
                return NotFound();
            }

            var viewModel = new NRCFormViewModel
            {
                Student = student,
                NRCTypeList = _context.NRCTypes.ToList(),
                RegionCodeMList = _context.NRCTownships.Select(t => t.RegionCode_M).Distinct().ToList(),
                TownshipList = _context.NRCTownships.ToList(),
                DepartmentList = _context.StudentDepartments.OrderBy(d => d.DepartmentName).ToList(),
                AcademicYearList = _context.AcademicYears.OrderByDescending(a => a.YearRange).ToList(),
                ProjectMembers = _context.ProjectMembers.Where(pm => !pm.IsDeleted).ToList()
            };

            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NRCFormViewModel model)
        {
            if (id != model.Student.Student_pkId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Reload lists before returning view
                model.NRCTypeList = _context.NRCTypes.ToList();
                model.RegionCodeMList = _context.NRCTownships.Select(t => t.RegionCode_M).Distinct().ToList();
                model.TownshipList = _context.NRCTownships.ToList();
                model.DepartmentList = _context.StudentDepartments.OrderBy(d => d.DepartmentName).ToList();
                model.AcademicYearList = _context.AcademicYears.OrderByDescending(a => a.YearRange).ToList();
                return View(model);
            }

            try
            {
                var studentInDb = await _context.Students.FindAsync(id);
                if (studentInDb == null)
                {
                    return NotFound();
                }

                // Update fields
                studentInDb.StudentName = model.Student.StudentName;
                studentInDb.RollNumber = model.Student.RollNumber;
                studentInDb.Email = model.Student.Email;
                studentInDb.PhoneNumber = model.Student.PhoneNumber;
                studentInDb.Department_pkID = model.Student.Department_pkID;
                studentInDb.AcademicYear_pkId = model.Student.AcademicYear_pkId;
                studentInDb.NRCType_pkId = model.Student.NRCType_pkId;
                studentInDb.NRC_pkId = model.Student.NRC_pkId;
                studentInDb.NRCNumber = model.Student.NRCNumber;
                studentInDb.CreatedBy = model.Student.CreatedBy;

                await _context.SaveChangesAsync();

                HttpContext.Session.SetString("SuccessMessage", "Student updated successfully!"); 
                return RedirectToAction("Dashboard");
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest("Unable to update student.");
            }
        }
        public async Task<IActionResult> Dashboard(int studentPage = 1, int projectPage = 1)
        {
            var rollNumber = HttpContext.Session.GetString("RollNumber");

            if (string.IsNullOrEmpty(rollNumber))
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login", "StudentLogin");
            }

            // Load the logged-in student
            var student = await _context.Students
                .Include(s => s.StudentDepartment)
                .Include(s => s.ProjectMembers)
                    .ThenInclude(pm => pm.Project)
                .Include(s => s.AcademicYear)
                .Include(s => s.NRCTownship)
                .Include(s => s.NRCType)
                .FirstOrDefaultAsync(s => s.RollNumber == rollNumber && !s.IsDeleted);

            if (student == null)
            {
                TempData["Error"] = "Student not found.";
                return RedirectToAction("Login", "StudentLogin");
            }

            // Replace your current 'students' query with this:
            //var students = new List<Student> { student }.ToPagedList(studentPage, 3);

            // Filter student list (for Student section in Dashboard) — can be filtered more later if needed
            var students = await _context.Students
                .Include(s => s.StudentDepartment)
                .Include(s => s.ProjectMembers)
                .Include(s => s.AcademicYear)
                .Include(s => s.NRCTownship)
                .Include(s => s.NRCType)
                .Where(s => !s.IsDeleted)
                .OrderByDescending(s => s.CreatedDate)
                .ToPagedListAsync(studentPage, 3);

            var projects = await _context.Projects
                 .Include(p => p.ProjectType)
                 .Include(p => p.Language)
                 .Include(p => p.Framework)
                 .Include(p => p.Company)
                 .Include(p => p.ProjectMembers)
                     .ThenInclude(pm => pm.Student)
                 .Where(p => !p.IsDeleted)
                 .OrderByDescending(p => p.ProjectSubmittedDate)
                 .ToPagedListAsync(projectPage, 3);


            // Build combined ViewModel
            var viewModel = new StudentDashboardViewModel
            {
                Students = students,
                Projects = projects,
                LoggedInStudent = student
            };

            return View(viewModel);
        }
        //public async Task<IActionResult> Dashboard(int studentPage = 1, int projectPage = 1)
        //{
        //    var rollNumber = HttpContext.Session.GetString("RollNumber");

        //    if (string.IsNullOrEmpty(rollNumber))
        //    {
        //        TempData["Error"] = "Session expired. Please login again.";
        //        return RedirectToAction("Login", "StudentLogin");
        //    }

        //    // Get the logged-in student with all details
        //    var student = await _context.Students
        //        .Include(s => s.StudentDepartment)
        //        .Include(s => s.ProjectMembers).ThenInclude(pm => pm.Project)
        //        .Include(s => s.AcademicYear)
        //        .Include(s => s.NRCTownship)
        //        .Include(s => s.NRCType)
        //        .FirstOrDefaultAsync(s => s.RollNumber == rollNumber && !s.IsDeleted);

        //    if (student == null)
        //    {
        //        TempData["Error"] = "Student not found.";
        //        return RedirectToAction("Login", "StudentLogin");
        //    }

        //    // ❗️Only return logged-in student as a PagedList
        //    var students = new List<Student> { student }.ToPagedList(studentPage, 1);

        //    // Get project IDs the student is part of
        //    var studentProjectIds = student.ProjectMembers
        //        .Where(pm => !pm.IsDeleted)
        //        .Select(pm => pm.Project_pkId)
        //        .Distinct()
        //        .ToList();

        //    // Load their project(s)
        //    var projects = await _context.Projects
        //        .Include(p => p.ProjectType)
        //        .Include(p => p.Language)
        //        .Include(p => p.Framework)
        //        .Include(p => p.Company)
        //        .Where(p => studentProjectIds.Contains(p.Project_pkId))
        //        .OrderByDescending(p => p.ProjectSubmittedDate)
        //        .ToPagedListAsync(projectPage, 3);

        //    var viewModel = new StudentDashboardViewModel
        //    {
        //        Students = students,
        //        Projects = projects,
        //        LoggedInStudent = student
        //    };

        //    return View(viewModel);
        //}


        //public async Task<IActionResult> Dashboard(int studentPage = 1, int projectPage = 1)
        //{
        //    var students = await _context.Students
        //        .Include(s => s.StudentDepartment)
        //        .Include(s => s.ProjectMembers)
        //        .Include(s => s.AcademicYear)
        //        .Include(s => s.NRCTownship)
        //        .Include(s => s.NRCType)
        //        .Where(s => !s.IsDeleted)
        //        .OrderByDescending(s => s.CreatedDate)
        //        .ToPagedListAsync(studentPage, 3);

        //    var projects = await _context.Projects
        //        .Include(p => p.ProjectType)
        //        .Include(p => p.Language)
        //        .Include(p => p.Framework)
        //        .Include(p => p.Company)
        //        .OrderByDescending(p => p.ProjectSubmittedDate)
        //        .ToPagedListAsync(projectPage, 3);

        //    var viewModel = new StudentDashboardViewModel
        //    {
        //        Students = students,
        //        Projects = projects
        //    };

        //    return View(viewModel);
        //}

        // GET: Student/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _context.Students
                .Include(s => s.StudentDepartment)
                .Include(s => s.ProjectMembers)
                .Include(s => s.AcademicYear)
                .Include(s => s.NRCTownship)
                .Include(s => s.NRCType)
                .Where(s => !s.IsDeleted)
                .FirstOrDefaultAsync(s => s.Student_pkId == id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student); // Show confirmation page
        }

        // POST: Student/DeleteConfirmed/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);

            if (student == null)
            {
                return NotFound();
            }

            student.IsDeleted = true;
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("SuccessMessage", "Student removed successfully!");
            return RedirectToAction(nameof(Dashboard));
        }

        public IActionResult Help()
        {
            // You can pass data to the view if needed, for now just return the view
            return View();
        }
    }
}
