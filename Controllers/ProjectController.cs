using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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

        public ProjectController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;           
            _env = env;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 3;
            var projects = _context.Projects
                .Include(p => p.ProjectType)
                .Include(p => p.Language)
                .Include(p => p.Framework)
                .Include(p => p.Company)
                .Include(p => p.Files)  // Added ProjectFiles to match your view usage
                .OrderByDescending(p => p.ProjectSubmittedDate);

            var pagedProjects = await projects.ToPagedListAsync(page, pageSize);

            return View(pagedProjects);
        }
        // GET: Project/Upload/5
        public async Task<IActionResult> Upload(int id)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectType)
                .Include(p => p.Language)
                .Include(p => p.Framework)
                .Include(p => p.Company)
                .Include(p => p.Files)
                .FirstOrDefaultAsync(p => p.Project_pkId == id);

            if (project == null)
                return NotFound();

            return View(project); // This will show full project info in the Upload view
        }

        // POST: Project/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(Project project)
        {
            var existingProject = await _context.Projects.FindAsync(project.Project_pkId);
            if (existingProject == null)
                return NotFound();

            // Mark project as submitted to teacher
            existingProject.Status = "Pending";
            existingProject.ProjectSubmittedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["UploadSuccess"] = "Project successfully uploaded and sent to teacher.";

            return RedirectToAction(nameof(Index));
        }



        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.ProjectType)
                .Include(p => p.Language)
                .Include(p => p.Framework)
                .Include(p => p.Company)
                .FirstOrDefaultAsync(m => m.Project_pkId == id);

            if (project == null) return NotFound();

            return View(project);
        }

        public IActionResult Create()
        {
            LoadDropdownData();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Project project, string CompanyAddress, string CompanyContact, string CompanyDescription, int? CompanyCity_pkId, IFormFile CompanyPhoto, List<IFormFile> projectFiles)
        {
            if (ModelState.IsValid)  // <-- fix: run only when model state is valid
            {
                // Set status and submission date
                project.Status = "Pending";
                project.ProjectSubmittedDate = DateTime.Now;

                // Update company details if company is selected
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

                            using (var stream = new FileStream(companyFilePath, FileMode.Create))
                            {
                                await CompanyPhoto.CopyToAsync(stream);
                            }

                            company.ImageFileName = uniqueCompanyFileName;
                        }

                        _context.Companies.Update(company);
                    }
                }

                // Save project to get Project_pkId
                _context.Projects.Add(project);
                await _context.SaveChangesAsync();

                // Save project files
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

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

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

                return RedirectToAction(nameof(Index));
            }

            // Reload dropdown data if model state invalid
            LoadDropdownData(project);
            return View(project);
        }




        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var project = await _context.Projects
                .Include(p => p.Company)
                .Include(p => p.Files) // Include existing files
                .FirstOrDefaultAsync(p => p.Project_pkId == id);

            if (project == null)
                return NotFound();

            LoadDropdownData(project);
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

            if (ModelState.IsValid) // FIX: Proceed only if valid
            {
                LoadDropdownData(project);
                return View(project);
            }

            try
            {
                // Update company info
                if (project.Company_pkId != 0)
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
                            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "companies");
                            Directory.CreateDirectory(uploadsFolder);

                            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(CompanyPhoto.FileName)}";
                            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await CompanyPhoto.CopyToAsync(stream);
                            }

                            company.ImageFileName = uniqueFileName;
                        }

                        _context.Companies.Update(company);
                    }
                }

                _context.Projects.Update(project);
                await _context.SaveChangesAsync();

                // Save new project files
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

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            var projectFile = new ProjectFile
                            {
                                Project_pkId = project.Project_pkId,
                                FilePath = $"/uploads/projects/{uniqueFileName}",
                                FileType = Path.GetExtension(file.FileName) // REQUIRED if not nullable
                            };

                            _context.ProjectFiles.Add(projectFile);
                        }
                    }

                    await _context.SaveChangesAsync();
                }

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





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> DeleteProjectFile(int id)
        {
            var file = await _context.ProjectFiles.FindAsync(id);
            if (file == null)
                return NotFound();

            // Delete file from disk
            var filePath = Path.Combine(_env.WebRootPath, file.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            // Remove from DB
            _context.ProjectFiles.Remove(file);
            await _context.SaveChangesAsync();

            // Redirect to Edit page of the associated project
            return RedirectToAction("Edit", new { id = file.Project_pkId });
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
                "ProjectType_pkId", "TypeName", selectedProject?.ProjectType_pkId
            );

            ViewBag.Languages = new SelectList(
                _context.Languages.OrderBy(l => l.LanguageName),
                "Language_pkId", "LanguageName", selectedProject?.Language_pkId
            );

            ViewBag.Frameworks = new SelectList(
                _context.Frameworks.OrderBy(f => f.FrameworkName),
                "Framework_pkId", "FrameworkName", selectedProject?.Framework_pkId
            );

            var companies = _context.Companies
                .Where(c => c.CompanyName != null)
                .ToList();

            ViewBag.Companies = new SelectList(
                companies, "Company_pkId", "CompanyName", selectedProject?.Company_pkId
            );

            ViewBag.CityList = new SelectList(
                _context.Cities.OrderBy(c => c.CityName),
                "City_pkId", "CityName"
            );
        }
    }
}
