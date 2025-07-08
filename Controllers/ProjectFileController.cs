using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using X.PagedList;

public class ProjectFileController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public ProjectFileController(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    // GET: List grouped by Project
    public IActionResult Index()
    {
        var groupedFiles = _context.ProjectFiles
            .Include(f => f.Project)
            .AsEnumerable() // Switch to client-side LINQ
            .GroupBy(f => f.Project.ProjectName) // Group by Project name
            .ToList();

        return View(groupedFiles);
    }

    // GET: ProjectFile/EditGroup/5
    public IActionResult EditGroup(int projectId)
    {
        var project = _context.Projects.FirstOrDefault(p => p.Project_pkId == projectId);
        if (project == null)
        {
            return NotFound();
        }

        var files = _context.ProjectFiles
            .Where(f => f.Project_pkId == projectId)
            .ToList();

        ViewBag.ProjectName = project.ProjectName;
        ViewBag.ProjectId = projectId;

        return View(files);
    }

    // POST: ProjectFile/EditGroup
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditGroup(List<ProjectFile> updatedFiles)
    {
        foreach (var file in updatedFiles)
        {
            var existing = await _context.ProjectFiles.FindAsync(file.ProjectFile_pkId);
            if (existing != null)
            {
                existing.UploadedBy = file.UploadedBy;
                // Add other editable fields here if needed
                _context.Update(existing);
            }
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }



    // GET: Upload form
    public IActionResult Create()
    {
        ViewBag.ProjectList = new SelectList(_context.Projects, "Project_pkId", "ProjectName");
        return View();
    }

    // POST: Upload multiple files
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int Project_pkId, List<IFormFile> files, string UploadedBy)
    {
        if (files == null || files.Count == 0)
        {
            ModelState.AddModelError("files", "Please select at least one file.");
        }
        if (string.IsNullOrWhiteSpace(UploadedBy))
        {
            ModelState.AddModelError("UploadedBy", "Uploader name is required.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.ProjectList = new SelectList(_context.Projects, "Project_pkId", "ProjectName", Project_pkId);
            return View();
        }

        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                var projectFile = new ProjectFile
                {
                    Project_pkId = Project_pkId,
                    FilePath = "/uploads/" + fileName,
                    FileType = file.ContentType,
                    FileSize = file.Length,
                    UploadedBy = UploadedBy,
                    UploadedAt = DateTime.Now
                };

                _context.ProjectFiles.Add(projectFile);
            }
        }
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // GET: Edit one file info
    public async Task<IActionResult> Edit(int id)
    {
        var projectFile = await _context.ProjectFiles.FindAsync(id);
        if (projectFile == null) return NotFound();

        ViewBag.ProjectList = new SelectList(_context.Projects, "Project_pkId", "ProjectName", projectFile.Project_pkId);
        return View(projectFile);
    }

    // POST: Edit one file info and optional replace file
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, int Project_pkId, string UploadedBy, IFormFile NewFile)
    {
        var projectFile = await _context.ProjectFiles.FindAsync(id);
        if (projectFile == null) return NotFound();

        if (string.IsNullOrWhiteSpace(UploadedBy))
        {
            ModelState.AddModelError("UploadedBy", "Uploader name is required.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.ProjectList = new SelectList(_context.Projects, "Project_pkId", "ProjectName", Project_pkId);
            return View(projectFile);
        }

        projectFile.Project_pkId = Project_pkId;
        projectFile.UploadedBy = UploadedBy;

        if (NewFile != null && NewFile.Length > 0)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Path.GetFileName(NewFile.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await NewFile.CopyToAsync(stream);

            projectFile.FilePath = "/uploads/" + fileName;
            projectFile.FileType = NewFile.ContentType;
            projectFile.FileSize = NewFile.Length;
            projectFile.UploadedAt = DateTime.Now;
        }

        _context.Update(projectFile);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // POST: Delete one file
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var projectFile = await _context.ProjectFiles.FindAsync(id);
        if (projectFile != null)
        {
            _context.ProjectFiles.Remove(projectFile);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
