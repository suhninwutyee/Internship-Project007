using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;


namespace ProjectManagementSystem.Controllers
{
    public class ProjectController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ProjectController> _logger;        
        public ProjectController(ApplicationDbContext context, IWebHostEnvironment env, ILogger<ProjectController> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;           
        }

        //   public async Task<IActionResult> Index(int page = 1)
        //   {
        //       var rollNumber = HttpContext.Session.GetString("RollNumber");
        //       if (string.IsNullOrEmpty(rollNumber))
        //       {
        //           return RedirectToAction("Login", "StudentLogin");
        //       }

        //       int pageSize = 3;
        //       var projects = _context.Projects
        //.Include(p => p.ProjectType)
        //.Include(p => p.Language)
        //.Include(p => p.Framework)
        //.Include(p => p.Company)
        //.Include(p => p.Files)
        //.Include(p => p.ProjectMembers)
        //    .ThenInclude(pm => pm.Student)
        //.OrderByDescending(p => p.ProjectSubmittedDate);


        //       var pagedProjects = await projects.ToPagedListAsync(page, pageSize);

        //       return View(pagedProjects);
        //   }

        public async Task<IActionResult> Index(int page = 1)
        {
            var rollNumber = HttpContext.Session.GetString("RollNumber");
            if (string.IsNullOrEmpty(rollNumber))
            {
                return RedirectToAction("Login", "StudentLogin");
            }

            int pageSize = 1; // Show only one project per page
            var projects = _context.Projects
                .Include(p => p.ProjectType)
                .Include(p => p.Language)
                .Include(p => p.Framework)
                .Include(p => p.Company)
                .Include(p => p.Files)
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.Student)
                .Where(p => p.ProjectMembers.Any(pm => pm.Student.Email.RollNumber == rollNumber)) // Filter by current student
                .OrderByDescending(p => p.ProjectSubmittedDate);

            var pagedProjects = await projects.ToPagedListAsync(page, pageSize);

            
            return View(pagedProjects);
        }

        [HttpGet]
        public JsonResult GetSuggestions(string term)
        {
            var suggestions = _context.Projects
                .Where(p => p.ProjectName.Contains(term))
                .OrderBy(p => p.ProjectName)
                .Select(p => p.ProjectName)
                .Distinct()
                .Take(5)
                .ToList();

            return Json(suggestions);
        }

        // GET: Project/Upload/5
        //public async Task<IActionResult> Upload(int id)
        //{
        //    var project = await _context.Projects
        //        .Include(p => p.ProjectType)
        //        .Include(p => p.Language)
        //        .Include(p => p.Framework)
        //        .Include(p => p.Company)
        //        .Include(p => p.Files)
        //        .FirstOrDefaultAsync(p => p.Project_pkId == id);

        //    if (project == null)
        //        return NotFound();

        //    if (project.Status == "Pending" || project.Status == "Approved")
        //    {
        //        TempData["UploadError"] = "You cannot upload again because the project is already submitted or approved.";
        //        return RedirectToAction(nameof(Index));
        //    }

        //    return View(project);
        //}


        // GET: Project/Upload/5
        public async Task<IActionResult> Upload(int id)
        {
            var rollNumber = HttpContext.Session.GetString("RollNumber");
            if (string.IsNullOrEmpty(rollNumber))
            {
                return RedirectToAction("Login", "StudentLogin");
            }
            var project = await _context.Projects
                .Include(p => p.ProjectType)
                .Include(p => p.Language)
                .Include(p => p.Framework)
                .Include(p => p.Company)
                .Include(p => p.Files)
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.Student)
                .FirstOrDefaultAsync(p => p.Project_pkId == id);

            if (project == null)
                return NotFound();

            if (project.Status == "Pending" || project.Status == "Approved")
            {
                TempData["UploadError"] = "You cannot upload again because the project is already submitted or approved.";
                return RedirectToAction(nameof(Index));
            }

            return View(project);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> UploadProj(int Project_pkId)
        {
            if (await IsSubmissionBlocked())
            {
                var blockingAnnouncement = await _context.Announcements
                    .FirstOrDefaultAsync(a => a.IsActive && a.BlocksSubmissions);

                TempData["Error"] = $"Project submissions are currently blocked: {blockingAnnouncement?.Title}";
                return RedirectToAction(nameof(Index));
            }

            var existingProject = await _context.Projects
                .Include(p => p.Language)
                .Include(p => p.ProjectType)
                .Include(p => p.Framework)
                .Include(p => p.Company)
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.Student)
                .FirstOrDefaultAsync(p => p.Project_pkId == Project_pkId);

            if (existingProject == null)
                return NotFound();

            // Prevent multiple uploads
            if (existingProject.Status == "Pending" || existingProject.Status == "Approved")
            {
                TempData["Error"] = "You cannot upload again. Wait for teacher feedback.";
                return RedirectToAction(nameof(Index));
            }

            // Mark project as submitted
            existingProject.Status = "Pending";
            existingProject.ProjectSubmittedDate = DateTime.Now;

            var leader = existingProject.ProjectMembers.FirstOrDefault(pm => pm.Role == "Leader")?.Student;
            var leaderName = leader?.StudentName ?? "Unknown Student";

            // Create notification for each team member
            foreach (var member in existingProject.ProjectMembers)
            {
                var notification = new Notification
                {
                    UserId = member.Student.Student_pkId,
                    Title = "Project Submitted",
                    Message = $"{leaderName} submitted the project '{existingProject.ProjectName}'",
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    NotificationType = "ProjectStatus",
                    Project_pkId = existingProject.Project_pkId
                };
                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Project submitted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Add similar notification logic for when a project is approved
        public async Task<IActionResult> ApproveProject(int id)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.Student)
                .FirstOrDefaultAsync(p => p.Project_pkId == id);

            if (project == null)
                return NotFound();

            project.Status = "Approved";

            // Create notification for each team member
            foreach (var member in project.ProjectMembers)
            {
                var notification = new Notification
                {
                    UserId = member.Student.Student_pkId,
                    Title = "Project Approved",
                    Message = $"Congratulations! Your project '{project.ProjectName}' has been approved.",
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    NotificationType = "ProjectStatus"
                };
                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int? id)
        {
            var rollNumber = HttpContext.Session.GetString("RollNumber");
            if (string.IsNullOrEmpty(rollNumber))
            {
                return RedirectToAction("Login", "StudentLogin");
            }

            if (id == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.ProjectType)
                .Include(p => p.Language)
                .Include(p => p.Framework)
                .Include(p => p.Company)
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.Student) // ✅ Include Student data for each ProjectMember
                .FirstOrDefaultAsync(m => m.Project_pkId == id);

            if (project == null) return NotFound();

            return View(project);
        }

    

   
        [HttpGet]
        public IActionResult AddMember(int projectId)
        {
            var rollNumber = HttpContext.Session.GetString("RollNumber");
            if (string.IsNullOrEmpty(rollNumber))
            {
                return RedirectToAction("Login", "StudentLogin");
            }
            try
            {
               
                var project = _context.Projects
                    .Include(p => p.ProjectType)
                    .Include(p => p.Language)
                    .Include(p => p.Framework)
                    .FirstOrDefault(p => p.Project_pkId == projectId);

                if (project == null)
                {
                    TempData["Error"] = "Project not found.";
                    return RedirectToAction("Dashboard", "Student");
                }

                var model = new AddMemberViewModel
                {
                    ProjectId = projectId,
                    ProjectName = project.ProjectName,
                    ProjectType = project.ProjectType,
                    Language = project.Language,
                    Framework = project.Framework
                };

                return View(model);
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "Error loading AddMember page");
                TempData["Error"] = "An error occurred while loading the page.";
                return RedirectToAction("Dashboard", "Student");
            }
        }
        // POST: Project/AddMember
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(AddMemberViewModel model)
        {
            var rollNumber = HttpContext.Session.GetString("RollNumber");
            if (string.IsNullOrEmpty(rollNumber))
            {
                return RedirectToAction("Login", "StudentLogin");
            }

            

            // Find student by RollNumber and EmailAddress
            var student = await _context.Students
                .Include(s => s.Email)
                .FirstOrDefaultAsync(s =>
                    s.Email.RollNumber == model.RollNumber &&
                    s.Email.EmailAddress == model.EmailAddress);

            if (student == null)
            {
                ModelState.AddModelError(string.Empty, "Student not found. Please check Roll Number and Email.");
                return View(model);
            }

            // Check if already added
            bool alreadyAdded = await _context.ProjectMembers
                .AnyAsync(pm => pm.Student_pkId == student.Student_pkId && pm.Project_pkId == model.ProjectId);

            if (alreadyAdded)
            {
                ModelState.AddModelError(string.Empty, "This student is already added to the project.");
                return View(model);
            }

            // Add member
            var newMember = new ProjectMember
            {
                Student_pkId = student.Student_pkId,
                Project_pkId = model.ProjectId,
                Role = "Member",
                IsDeleted = false
            };

            _context.ProjectMembers.Add(newMember);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Team member added successfully.";
            return RedirectToAction("Dashboard", "Student");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int projectId, int studentId)
        {
            // Session check
            var rollNumber = HttpContext.Session.GetString("RollNumber");
            if (string.IsNullOrEmpty(rollNumber))
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login", "StudentLogin");
            }

            try
            {
                // Get current user and validate
                var currentUser = await _context.Students
                    .Include(s => s.Email)
                    .FirstOrDefaultAsync(s => s.Email.RollNumber == rollNumber);

                if (currentUser == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("Login", "StudentLogin");
                }

                // Get project with members
                var project = await _context.Projects
                    .Include(p => p.ProjectMembers)
                        .ThenInclude(pm => pm.Student)
                            .ThenInclude(s => s.Email)
                    .FirstOrDefaultAsync(p => p.Project_pkId == projectId);

                if (project == null)
                {
                    TempData["Error"] = "Project not found.";
                    return RedirectToAction("Dashboard", "Student");
                }

                // Verify current user is the project leader
                var currentUserIsLeader = project.ProjectMembers
                    .Any(pm => pm.Student_pkId == currentUser.Student_pkId && pm.Role == "Leader");

                if (!currentUserIsLeader)
                {
                    TempData["Error"] = "Only project leaders can remove members.";
                    return RedirectToAction("Dashboard", "Student");
                }

                // Find member to remove
                var memberToRemove = await _context.ProjectMembers
                    .Include(pm => pm.Student)
                    .FirstOrDefaultAsync(pm =>
                        pm.Project_pkId == projectId &&
                        pm.Student_pkId == studentId &&
                        pm.Role != "Leader");

                if (memberToRemove == null)
                {
                    TempData["Error"] = "Member not found or cannot be removed (cannot remove leaders).";
                    return RedirectToAction("Dashboard", "Student");
                }

                // Prevent removing yourself
                if (memberToRemove.Student_pkId == currentUser.Student_pkId)
                {
                    TempData["Error"] = "You cannot remove yourself as leader. Transfer leadership first.";
                    return RedirectToAction("Dashboard", "Student");
                }

                // Remove member
                _context.ProjectMembers.Remove(memberToRemove);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"{memberToRemove.Student?.StudentName ?? "Member"} has been removed from the project.";
                return RedirectToAction("Dashboard", "Student");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error while removing member");
                TempData["Error"] = "A database error occurred while removing the member.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error removing member");
                TempData["Error"] = "An unexpected error occurred while removing the member.";
            }

            return RedirectToAction("Dashboard", "Student");
        }

       
        public IActionResult Create()
        {
            var rollNumber = HttpContext.Session.GetString("RollNumber");
            if (string.IsNullOrEmpty(rollNumber))
            {
                return RedirectToAction("Login", "StudentLogin");
            }
            LoadDropdownData();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Project project, string CompanyAddress, string CompanyContact, string CompanyDescription, int? CompanyCity_pkId, IFormFile CompanyPhoto, List<IFormFile> projectFiles)
        {

            
                project.Status = "Draft";
                project.ProjectSubmittedDate= null;

                if (project.Company_pkId != null && project.Company_pkId != 0)
                {
                    var company = await _context.Companies.FindAsync(project.Company_pkId);
                    if (company != null)
                    {
                        company.Address = CompanyAddress;
                        company.Contact = CompanyContact;
                        company.Description = CompanyDescription;
                        company.City_pkId = CompanyCity_pkId;

                        if (CompanyPhoto != null && CompanyPhoto.Length > 0)
                        {
                            var companyFolder = Path.Combine(_env.WebRootPath, "images", "companies");
                            Directory.CreateDirectory(companyFolder);

                            var uniqueCompanyFileName = $"{Guid.NewGuid()}{Path.GetExtension(CompanyPhoto.FileName)}";
                            var companyFilePath = Path.Combine(companyFolder, uniqueCompanyFileName);

                            using var stream = new FileStream(companyFilePath, FileMode.Create);
                            await CompanyPhoto.CopyToAsync(stream);

                            company.ImageFileName = uniqueCompanyFileName;
                        }

                        _context.Companies.Update(company);
                    }
                }

                _context.Projects.Add(project);
                await _context.SaveChangesAsync();

                // Add logged-in student as Leader in ProjectMembers
                var rollNumber = HttpContext.Session.GetString("RollNumber");
                if (!string.IsNullOrEmpty(rollNumber))
                {
                    var student = await _context.Students
                        .Include(s => s.Email)
                        .FirstOrDefaultAsync(s => s.Email.RollNumber == rollNumber && !s.IsDeleted);

                    if (student != null)
                    {
                        var projectMember = new ProjectMember
                        {
                            Student_pkId = student.Student_pkId,
                            Project_pkId = project.Project_pkId,

                            Role = "Leader",
                            IsDeleted = false
                        };

                        _context.ProjectMembers.Add(projectMember);
                        await _context.SaveChangesAsync();
                    }
                }

                if (projectFiles != null && projectFiles.Count > 0)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "projects");
                    Directory.CreateDirectory(uploadsFolder);

                    foreach (var file in projectFiles)
                    {
                        if (file.Length > 0)
                        {
                            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using var stream = new FileStream(filePath, FileMode.Create);
                            await file.CopyToAsync(stream);

                            var projectFile = new ProjectFile
                            {
                                Project_pkId = project.Project_pkId,
                                FilePath = $"/uploads/projects/{uniqueFileName}",
                                FileType = Path.GetExtension(file.FileName)
                            };
                            _context.ProjectFiles.Add(projectFile);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Dashboard", "Student");
            
        }


        public async Task<IActionResult> Edit(int? id)
        {
            var rollNumber = HttpContext.Session.GetString("RollNumber");
            if (string.IsNullOrEmpty(rollNumber))
            {
                return RedirectToAction("Login", "StudentLogin");
            }

            if (id == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.ProjectType)
                .Include(p => p.Language)
                .Include(p => p.Framework)
                .Include(p => p.Company)
                .Include(p => p.Files)
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.Student)
                .FirstOrDefaultAsync(p => p.Project_pkId == id);

            if (project == null) return NotFound();
            if (project.Status == "Pending" || project.Status == "Approved")
            {
                TempData["Error"] = "You cannot edit a project that is pending or approved. Wait for teacher feedback.";
                return RedirectToAction(nameof(Index));
            }

            LoadDropdownData(project); // ✅ required
            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Project project,
            string CompanyAddress,
            string CompanyContact,
            string CompanyDescription,
            int? CompanyCity_pkId,
            IFormFile CompanyPhoto,
            List<IFormFile> projectFiles)
        {
            if (id != project.Project_pkId)
                return NotFound();
            var existingProject = await _context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Project_pkId == id);
            if (existingProject == null)
                return NotFound();


            if (existingProject.Status == "Pending" || existingProject.Status == "Approved")
            {
                TempData["Error"] = "You cannot edit a project that is pending or approved. Wait for teacher feedback.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                LoadDropdownData(project);
                return View(project);
            }

            try
            {
                // Fetch the existing project with related data
                var projectInDb = await _context.Projects
                    .Include(p => p.Company)
                    .Include(p => p.Files)
                    .FirstOrDefaultAsync(p => p.Project_pkId == id);

                if (projectInDb == null)
                    return NotFound();

                // Update project fields explicitly
                projectInDb.ProjectName = project.ProjectName;
                projectInDb.SupervisorName = project.SupervisorName;
                projectInDb.Description = project.Description;
                projectInDb.ProjectType_pkId = project.ProjectType_pkId;
                projectInDb.Language_pkId = project.Language_pkId;
                projectInDb.Framework_pkId = project.Framework_pkId;
                projectInDb.Company_pkId = project.Company_pkId;
                projectInDb.ProjectSubmittedDate = project.ProjectSubmittedDate;
                projectInDb.CreatedBy = project.CreatedBy;

                // Update company info if company selected
                if (project.Company_pkId != null && project.Company_pkId != 0)
                {
                    var company = await _context.Companies.FindAsync(project.Company_pkId);
                    if (company != null)
                    {
                        company.Address = CompanyAddress;
                        company.Contact = CompanyContact;
                        company.Description = CompanyDescription;
                        company.City_pkId = CompanyCity_pkId;

                        // Handle company photo upload
                        if (CompanyPhoto != null && CompanyPhoto.Length > 0)
                        {
                            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "companies");
                            Directory.CreateDirectory(uploadsFolder);

                            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(CompanyPhoto.FileName)}";
                            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using var stream = new FileStream(filePath, FileMode.Create);
                            await CompanyPhoto.CopyToAsync(stream);

                            company.ImageFileName = uniqueFileName;
                        }

                        _context.Companies.Update(company);
                    }
                }

                _context.Projects.Update(projectInDb);
                await _context.SaveChangesAsync();

                // Handle project files upload
                if (projectFiles != null && projectFiles.Count > 0)
                {
                    var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "projects");
                    Directory.CreateDirectory(uploadPath);

                    foreach (var file in projectFiles)
                    {
                        if (file.Length > 0)
                        {
                            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                            var filePath = Path.Combine(uploadPath, uniqueFileName);

                            using var stream = new FileStream(filePath, FileMode.Create);
                            await file.CopyToAsync(stream);

                            var projectFile = new ProjectFile
                            {
                                Project_pkId = projectInDb.Project_pkId,
                                FilePath = $"/uploads/projects/{uniqueFileName}",
                                FileType = Path.GetExtension(file.FileName)
                            };

                            _context.ProjectFiles.Add(projectFile);
                        }
                    }
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Projects.Any(e => e.Project_pkId == project.Project_pkId))
                    return NotFound();
                else
                    throw;
            }
        }





        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    var project = await _context.Projects.FindAsync(id);
        //    if (project == null)
        //        return NotFound();

        //    if (project.Status == "Pending" || project.Status == "Approved")
        //    {
        //        TempData["Error"] = "You cannot delete a project that is pending or approved. Wait for teacher feedback.";
        //        return RedirectToAction(nameof(Index));
        //    }

        //    _context.Projects.Remove(project);
        //    await _context.SaveChangesAsync();

        //    return RedirectToAction(nameof(Index));
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectMembers)
                .FirstOrDefaultAsync(p => p.Project_pkId == id);

            if (project == null)
                return NotFound();

            if (project.Status == "Pending" || project.Status == "Approved")
            {
                TempData["Error"] = "You cannot delete a project that is pending or approved. Wait for teacher feedback.";
                return RedirectToAction(nameof(Index));
            }

            // First remove project members
            if (project.ProjectMembers.Any())
            {
                _context.ProjectMembers.RemoveRange(project.ProjectMembers);
            }

            // Then remove the project
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Project deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteProjectFile(int id)
        {
            var file = await _context.ProjectFiles.FindAsync(id);
            if (file == null) return NotFound();

            var filePath = Path.Combine(_env.WebRootPath, file.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.ProjectFiles.Remove(file);
            await _context.SaveChangesAsync();

            return RedirectToAction("Edit", new { id = file.Project_pkId });
        }

        [HttpGet]
        public JsonResult GetLanguagesByProjectType(int projectTypeId)
        {
            var languages = _context.Languages
                .Where(l => l.ProjectType_pkId == projectTypeId)
                .OrderBy(l => l.LanguageName)
                .Select(l => new
                {
                    language_pkId = l.Language_pkId,
                    languageName = l.LanguageName
                })
                .ToList();

            return Json(languages);
        }

        [HttpGet]
        public JsonResult GetFrameworksByLanguage(int languageId)
        {
            var frameworks = _context.Frameworks
                .Where(f => f.Language_pkId == languageId)
                .OrderBy(f => f.FrameworkName)
                .Select(f => new
                {
                    framework_pkId = f.Framework_pkId,
                    frameworkName = f.FrameworkName
                })
                .ToList();

            return Json(frameworks);
        }

        [HttpGet]
        public async Task<JsonResult> GetCompanyInfo(int companyId)
        {
            var company = await _context.Companies.FindAsync(companyId);
            if (company == null) return Json(null);

            return Json(new
            {
                address = company.Address,
                contact = company.Contact,
                description = company.Description,
                city_pkId = company.City_pkId,
                imageFileName = company.ImageFileName
            });
        }

        private void LoadDropdownData(Project? selectedProject = null)
        {
            ViewBag.ProjectTypes = new SelectList(
                _context.ProjectTypes.OrderBy(p => p.TypeName),
                "ProjectType_pkId",
                "TypeName",
                selectedProject?.ProjectType_pkId
            );

            // ✅ Show only related languages for selected project type
            if (selectedProject?.ProjectType_pkId != null)
            {
                var relatedLanguages = _context.Languages
                    .Where(l => l.ProjectType_pkId == selectedProject.ProjectType_pkId)
                    .OrderBy(l => l.LanguageName)
                    .ToList();

                ViewBag.Languages = new SelectList(
                    relatedLanguages,
                    "Language_pkId",
                    "LanguageName",
                    selectedProject.Language_pkId
                );
            }
            else
            {
                ViewBag.Languages = new SelectList(
                    Enumerable.Empty<SelectListItem>(),
                    "Language_pkId",
                    "LanguageName"
                );
            }

            ViewBag.Frameworks = new SelectList(
                _context.Frameworks.OrderBy(f => f.FrameworkName),
                "Framework_pkId",
                "FrameworkName",
                selectedProject?.Framework_pkId
            );

            ViewBag.Companies = new SelectList(
                _context.Companies
                    .Where(c => !string.IsNullOrEmpty(c.CompanyName))
                    .OrderBy(c => c.CompanyName),
                "Company_pkId",
                "CompanyName",
                selectedProject?.Company_pkId
            );

            ViewBag.CityList = new SelectList(
                _context.Cities.OrderBy(c => c.CityName),
                "City_pkId",
                "CityName"
            );
        }

        private async Task<bool> IsSubmissionBlocked()
        {
            return await _context.Announcements
                .AnyAsync(a => a.IsActive &&
                       a.BlocksSubmissions &&
                       DateTime.Now >= a.StartDate &&
                       (a.ExpiryDate == null || DateTime.Now <= a.ExpiryDate));
        }

    }
}