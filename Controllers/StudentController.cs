using Microsoft.AspNetCore.Hosting;
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
        private readonly IWebHostEnvironment _env;
        public StudentController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;

        }

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
                AcademicYearList = years,
            };

            // Read session values for Leader role
            var roll = HttpContext.Session.GetString("RollNumber");
            var email = HttpContext.Session.GetString("EmailAddress");
            var role = HttpContext.Session.GetString("UserRole");

            if (!string.IsNullOrEmpty(role) && role == "Leader")
            {
                // Initialize Email object if null to avoid NullReferenceException
                if (viewModel.Student.Email == null)
                {
                    viewModel.Student.Email = new Email();
                   
                }

                viewModel.Student.Email.RollNumber = roll;
                viewModel.Student.Email.EmailAddress = email;
                ViewBag.NextAction = "CreateProject"; // redirect target for Leader
            }

            return View(viewModel);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(NRCFormViewModel model, IFormFile? ProfilePhoto, string? nextAction)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        // Reload dropdowns if validation fails
        //        model.NRCTypeList = _context.NRCTypes.ToList();
        //        model.RegionCodeMList = _context.NRCTownships.Select(t => t.RegionCode_M).Distinct().ToList();
        //        model.TownshipList = _context.NRCTownships.ToList();
        //        model.DepartmentList = _context.StudentDepartments.OrderBy(d => d.DepartmentName).ToList();
        //        model.AcademicYearList = _context.AcademicYears.OrderByDescending(y => y.YearRange).ToList();
        //        return View(model);
        //    }

        //    if (ProfilePhoto != null && ProfilePhoto.Length > 0)
        //    {
        //        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/students");

        //        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

        //        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ProfilePhoto.FileName);
        //        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        //        using (var fileStream = new FileStream(filePath, FileMode.Create))
        //        {
        //            await ProfilePhoto.CopyToAsync(fileStream);
        //        }

        //        model.Student.ProfilePhotoUrl = "/uploads/students/" + uniqueFileName;
        //    }

        //    // Set additional fields
        //    model.Student.CreatedDate = DateTime.Now;
        //    model.Student.IsDeleted = false;

        //    // Get RollNumber and Email from session
        //    var roll = HttpContext.Session.GetString("RollNumber");
        //    var email = HttpContext.Session.GetString("EmailAddress");

        //    var emailEntry = _context.Emails.FirstOrDefault(e =>
        //        e.RollNumber == roll && e.EmailAddress == email && !e.IsDeleted);

        //    if (emailEntry == null)
        //    {
        //        TempData["Error"] = "Email record not found. Please log in again.";
        //        return RedirectToAction("Login", "StudentLogin");
        //    }

        //    model.Student.Email_PkId = emailEntry.Email_PkId;
        //    model.Student.CreatedBy = roll;

        //    _context.Students.Add(model.Student);
        //    await _context.SaveChangesAsync();

        //    // Save student pkId in session if needed later
        //    HttpContext.Session.SetInt32("Student_pkId", model.Student.Student_pkId);

        //    // Redirect Leader to Project Creation page after Student creation
        //    if (!string.IsNullOrEmpty(nextAction) && nextAction == "CreateProject")
        //    {
        //        return RedirectToAction("Create", "Project");
        //    }

        //    // For other roles, redirect to Student Dashboard or appropriate page
        //    return RedirectToAction("Dashboard", "Student");
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NRCFormViewModel model, IFormFile? ProfilePhoto, string? nextAction)
        {
            if (ModelState.IsValid)
            {
                model.NRCTypeList = _context.NRCTypes.ToList();
                model.RegionCodeMList = _context.NRCTownships.Select(t => t.RegionCode_M).Distinct().ToList();
                model.TownshipList = _context.NRCTownships.ToList();
                model.DepartmentList = _context.StudentDepartments.OrderBy(d => d.DepartmentName).ToList();
                model.AcademicYearList = _context.AcademicYears.OrderByDescending(y => y.YearRange).ToList();

                return View(model);
            }

            // ---------------------------
            // 🖼️ Profile Photo Upload Start
            // ---------------------------
            if (ProfilePhoto != null && ProfilePhoto.Length > 0)
            {
                // Folder path
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "students");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Unique filename
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ProfilePhoto.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file physically
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfilePhoto.CopyToAsync(fileStream);
                }

                // Save relative path to DB
                model.Student.ProfilePhotoUrl = "/uploads/students/" + uniqueFileName;
            }
            else
            {
                // Default profile photo
                model.Student.ProfilePhotoUrl = "/uploads/students/default-avatar.png";
            }
            // ---------------------------
            // 🖼️ Profile Photo Upload End
            // ---------------------------

            // Set additional fields
            model.Student.CreatedDate = DateTime.Now;
            model.Student.IsDeleted = false;

            // Get RollNumber and Email from session
            var roll = HttpContext.Session.GetString("RollNumber");
            var email = HttpContext.Session.GetString("EmailAddress");

            var emailEntry = _context.Emails.FirstOrDefault(e =>
                e.RollNumber == roll && e.EmailAddress == email && !e.IsDeleted);

            if (emailEntry == null)
            {
                TempData["Error"] = "Email record not found. Please log in again.";
                return RedirectToAction("Login", "StudentLogin");
            }

            model.Student.Email_PkId = emailEntry.Email_PkId;
            model.Student.CreatedBy = roll;

            // Save student to DB
            _context.Students.Add(model.Student);
            await _context.SaveChangesAsync();

            // Save student id to session
            HttpContext.Session.SetInt32("Student_pkId", model.Student.Student_pkId);

            // Redirect by role or next action
            if (!string.IsNullOrEmpty(nextAction) && nextAction == "CreateProject")
            {
                return RedirectToAction("Create", "Project");
            }

            // Default redirect
            return RedirectToAction("Dashboard", "Student");
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
                .Include(s => s.Email)
                .Include(s => s.NRCTownship)
                .Include(s => s.NRCType)
                .Include(s => s.StudentDepartment)
                
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
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, NRCFormViewModel model)
        //{
        //    if (id != model.Student.Student_pkId)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        // Reload lists before returning view
        //        model.NRCTypeList = _context.NRCTypes.ToList();
        //        model.RegionCodeMList = _context.NRCTownships.Select(t => t.RegionCode_M).Distinct().ToList();
        //        model.TownshipList = _context.NRCTownships.ToList();
        //        model.DepartmentList = _context.StudentDepartments.OrderBy(d => d.DepartmentName).ToList();
        //        model.AcademicYearList = _context.AcademicYears.OrderByDescending(a => a.YearRange).ToList();
        //        return View(model);
        //    }

        //    try
        //    {
        //        var studentInDb = await _context.Students.FindAsync(id);
        //        if (studentInDb == null)
        //        {
        //            return NotFound();
        //        }

        //        // Update fields
        //        //studentInDb.StudentName = model.Student.StudentName;
        //        //studentInDb.RollNumber = model.Student.RollNumber;
        //        studentInDb.Email = model.Student.Email;
        //        studentInDb.PhoneNumber = model.Student.PhoneNumber;
        //        studentInDb.Department_pkID = model.Student.Department_pkID;
        //        //studentInDb.AcademicYear_pkId = model.Student.AcademicYear_pkId;
        //        studentInDb.NRCType_pkId = model.Student.NRCType_pkId;
        //        studentInDb.NRC_pkId = model.Student.NRC_pkId;
        //        studentInDb.NRCNumber = model.Student.NRCNumber;
        //        studentInDb.CreatedBy = model.Student.CreatedBy;

        //        await _context.SaveChangesAsync();

        //        HttpContext.Session.SetString("SuccessMessage", "Student updated successfully!"); 
        //        return RedirectToAction("Dashboard");
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        return BadRequest("Unable to update student.");
        //    }
        //}
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, NRCFormViewModel model)
        //{
        //    if (id != model.Student.Student_pkId)
        //    {
        //        return NotFound();
        //    }

        //    // Reload lists before returning view if ModelState is invalid
        //    model.NRCTypeList = _context.NRCTypes.ToList();
        //    model.RegionCodeMList = _context.NRCTownships.Select(t => t.RegionCode_M).Distinct().ToList();
        //    model.TownshipList = _context.NRCTownships.ToList();
        //    model.DepartmentList = _context.StudentDepartments.OrderBy(d => d.DepartmentName).ToList();
        //    model.AcademicYearList = _context.AcademicYears.OrderByDescending(a => a.YearRange).ToList();

        //    if (ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    try
        //    {
        //        var studentInDb = await _context.Students.FindAsync(id);
        //        if (studentInDb == null)
        //        {
        //            return NotFound();
        //        }

        //        // Update fields
        //        //studentInDb.StudentName = model.Student.StudentName;
        //        //studentInDb.RollNumber = model.Student.RollNumber;
        //        studentInDb.Email = model.Student.Email;
        //        studentInDb.PhoneNumber = model.Student.PhoneNumber;
        //        studentInDb.Department_pkID = model.Student.Department_pkID;
        //        //studentInDb.AcademicYear_pkId = model.Student.AcademicYear_pkId;
        //        studentInDb.NRCType_pkId = model.Student.NRCType_pkId;
        //        studentInDb.NRC_pkId = model.Student.NRC_pkId;
        //        studentInDb.NRCNumber = model.Student.NRCNumber;
        //        studentInDb.CreatedBy = model.Student.CreatedBy;

        //        // ---------------------------
        //        // Profile Photo Upload Start
        //        // ---------------------------
        //        if (model.ProfilePhoto != null && model.ProfilePhoto.Length > 0)
        //        {
        //            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/students");
        //            if (!Directory.Exists(uploadsFolder))
        //                Directory.CreateDirectory(uploadsFolder);

        //            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfilePhoto.FileName);
        //            var filePath = Path.Combine(uploadsFolder, fileName);

        //            using (var stream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await model.ProfilePhoto.CopyToAsync(stream);
        //            }

        //            // Save relative path to DB
        //            studentInDb.ProfilePhotoUrl = "/uploads/students/" + fileName;
        //        }
        //        // ---------------------------
        //        // Profile Photo Upload End
        //        // ---------------------------

        //        await _context.SaveChangesAsync();

        //        HttpContext.Session.SetString("SuccessMessage", "Student updated successfully!");
        //        return RedirectToAction("Dashboard");
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        return BadRequest("Unable to update student.");
        //    }
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NRCFormViewModel model)
        {
            if (id != model.Student.Student_pkId)
                return NotFound();

            // Reload dropdown lists regardless of ModelState
            model.NRCTypeList = _context.NRCTypes.ToList();
            model.RegionCodeMList = _context.NRCTownships.Select(t => t.RegionCode_M).Distinct().ToList();
            model.TownshipList = _context.NRCTownships.ToList();
            model.DepartmentList = _context.StudentDepartments.OrderBy(d => d.DepartmentName).ToList();
            model.AcademicYearList = _context.AcademicYears.OrderByDescending(a => a.YearRange).ToList();

            if (ModelState.IsValid)
            {
                // Return view with validation errors and reloaded dropdowns
                return View(model);
            }

            try
            {
                var studentInDb = await _context.Students.FindAsync(id);
                if (studentInDb == null)
                    return NotFound();

                // Update student fields
                //studentInDb.StudentName = model.Student.StudentName;
                //studentInDb.RollNumber = model.Student.RollNumber;
                studentInDb.Email = model.Student.Email;
                studentInDb.PhoneNumber = model.Student.PhoneNumber;
                studentInDb.Department_pkID = model.Student.Department_pkID;
                studentInDb.AcademicYear_pkId = model.Student.AcademicYear_pkId;
                studentInDb.NRCType_pkId = model.Student.NRCType_pkId;
                studentInDb.NRC_pkId = model.Student.NRC_pkId;
                studentInDb.NRCNumber = model.Student.NRCNumber;
                studentInDb.CreatedBy = model.Student.CreatedBy;

                // ---------------------------
                // Profile Photo Upload
                // ---------------------------
                if (model.ProfilePhoto != null && model.ProfilePhoto.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "students");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ProfilePhoto.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfilePhoto.CopyToAsync(stream);
                    }

                    // Save relative path to DB
                    studentInDb.ProfilePhotoUrl = $"/uploads/students/{uniqueFileName}";
                }

                _context.Students.Update(studentInDb);
                await _context.SaveChangesAsync();

                HttpContext.Session.SetString("SuccessMessage", "Student updated successfully!");
                return RedirectToAction("Dashboard");
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest("Unable to update student due to concurrency issue.");
            }
            catch (Exception ex)
            {
                // General error handling
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

        public async Task<IActionResult> Dashboard()
        {
            // 1. Authentication and Session Check
            var studentId = HttpContext.Session.GetInt32("Student_pkId");
            if (studentId == null)
            {
                TempData["Error"] = "Session expired. Please log in again.";
                return RedirectToAction("Login", "StudentLogin");
            }

            // 2. Load Student with Related Data
            var student = await _context.Students
                .Include(s => s.Email)
                .Include(s => s.StudentDepartment)
                .Include(s => s.NRCTownship)
                .Include(s => s.NRCType)
                .Include(s => s.AcademicYear)
                .FirstOrDefaultAsync(s => s.Student_pkId == studentId);

            if (student == null)
            {
                TempData["Error"] = "Student not found.";
                return RedirectToAction("Login", "StudentLogin");
            }

            var projects = await _context.Projects
                .Where(p => p.SubmittedByStudent_pkId == studentId ||
                           p.ProjectMembers.Any(pm => pm.Student_pkId == studentId && !pm.IsDeleted))
                //.Include(p => p.ProjectType)
                .Include(p => p.Language)
                .Include(p => p.Framework)
                .Include(p => p.Company)
                    .ThenInclude(c => c.City)
                .Include(p => p.Files)
                //.Include(p => p.ProjectMembers)
                    //.ThenInclude(pm => pm.Student)
                        //.ThenInclude(s => s.Email)
                //.Include(p => p.ProjectMembers)
                //    .ThenInclude(pm => pm.Student)
                //        .ThenInclude(s => s.StudentDepartment)
                .OrderByDescending(p => p.ProjectSubmittedDate)
                .AsNoTracking()
                .ToListAsync();

            // 4. Load All Team Members for Leader Projects
            var leaderProjectIds = projects
                .Where(p => p.SubmittedByStudent_pkId == studentId)
                .Select(p => p.Project_pkId)
                .ToList();

            var allTeamMembers = await _context.ProjectMembers
                .Where(pm => leaderProjectIds.Contains((int)pm.Project_pkId) && !pm.IsDeleted)
                .Include(pm => pm.Student)
                    .ThenInclude(s => s.Email)
                .Include(pm => pm.Student)
                    .ThenInclude(s => s.StudentDepartment)
                .AsNoTracking()
                .ToListAsync();

            // 5. Mark Leader Role in Teams
            foreach (var project in projects.Where(p => p.SubmittedByStudent_pkId == studentId))
            {
                var leaderMember = allTeamMembers.FirstOrDefault(m =>
                    m.Project_pkId == project.Project_pkId &&
                    m.Student_pkId == studentId);

                if (leaderMember != null)
                {
                    leaderMember.Role = "Leader";
                    //leaderMember.RoleDescription = leaderMember.RoleDescription ?? "Manage project and assign members";
                }
            }

            // 6. Calculate Submission Status
            var submissionStatus = new ProjectSubmissionStatus
            {
                TotalProjects = projects.Count,
                DraftProjects = projects.Count(p => p.Status == "Draft"),
                PendingProjects = projects.Count(p => p.Status == "Pending"),
                ApprovedProjects = projects.Count(p => p.Status == "Approved"),
                RejectedProjects = projects.Count(p => p.Status == "Rejected"),
                RevisionRequired = projects.Count(p => p.Status == "Revision Required")
            };

            var notifications = await _context.Notifications
              .Where(n => n.UserId == student.Student_pkId)
              .OrderByDescending(n => n.CreatedAt)
              .ToListAsync();

            // 7. Determine if the logged-in student is a member
            var memberInfo = await _context.ProjectMembers
                .Include(pm => pm.Project)
                .ThenInclude(p => p.ProjectMembers)
                    .ThenInclude(m => m.Student)
                    .ThenInclude(s => s.Email)
                .FirstOrDefaultAsync(pm => pm.Student_pkId == studentId && !pm.IsDeleted);

            bool isMember = false;
            string? memberProjectName = null;
            string? memberResponsibility = null;
            string? leaderName = null;
            string? leaderEmail = null;

            if (memberInfo != null && memberInfo.Project != null)
            {
                isMember = true;
                memberProjectName = memberInfo.Project.ProjectName;
                memberResponsibility = memberInfo.RoleDescription;

                // Find the Leader of this project
                var leader = memberInfo.Project.ProjectMembers
                    .FirstOrDefault(m => m.Role == "Leader" && !m.IsDeleted);

                if (leader != null)
                {
                    leaderName = leader.Student?.StudentName;
                    leaderEmail = leader.Student?.Email?.EmailAddress;
                }
            }

            var dashboardViewModel = new StudentDashboardViewModel
            {
                Student = student,
                Projects = projects,
                TeamMembers = allTeamMembers,
                SubmissionStatus = submissionStatus,
                Notifications = notifications,
                LeaderProjects = projects.Where(p => p.SubmittedByStudent_pkId == studentId).ToList(),
                IsMember = isMember,
                MemberProjectName = memberProjectName,
                MemberResponsibility = memberResponsibility,
                LeaderName = leaderName,
                LeaderEmail = leaderEmail
            };

            return View(dashboardViewModel);
        }

     


        // GET: Student/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _context.Students
                .Include(s => s.StudentDepartment)
                .Include(s => s.ProjectMembers)
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
