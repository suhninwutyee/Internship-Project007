using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.Controllers
{
    public class ProjectApprovalController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectApprovalController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string statusFilter = "all", string searchString = "", int pageNumber = 1)
        {
            int pageSize = 3; // Items per page

            var query = _context.Projects
                .Include(p => p.Company)
                .Include(p => p.ProjectType)
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.Student)
                .Where(p => p.IsDeleted == null || p.IsDeleted == false);

            // Apply search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p =>
                    p.ProjectName.Contains(searchString) ||
                    p.CreatedBy.Contains(searchString) ||
                    p.ProjectMembers.Any(pm => pm.Student.StudentName.Contains(searchString))
                );
            }

            // Apply status filter
            if (statusFilter != "all")
            {
                query = query.Where(p => p.Status == statusFilter);
            }

            // Get counts and paginated results
            var totalCount = await query.CountAsync();
            var projects = await query
                .OrderByDescending(p => p.ProjectSubmittedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Set ViewBag values
            ViewBag.TotalCount = await _context.Projects.CountAsync(p => p.IsDeleted == null || p.IsDeleted == false);
            ViewBag.PendingCount = await _context.Projects.CountAsync(p => p.Status == "Pending" && (p.IsDeleted == null || p.IsDeleted == false));
            ViewBag.ApprovedCount = await _context.Projects.CountAsync(p => p.Status == "Approved" && (p.IsDeleted == null || p.IsDeleted == false));
            ViewBag.RejectedCount = await _context.Projects.CountAsync(p => p.Status == "Rejected" && (p.IsDeleted == null || p.IsDeleted == false));
            ViewBag.CurrentFilter = statusFilter;
            ViewBag.SearchString = searchString;
            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.PageTitle = statusFilter == "all" ? "All Projects" : $"{statusFilter} Projects";

            return View(projects);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve([FromBody] int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return Json(new { success = false, message = "Project not found." });

            project.Status = "Approved";
            project.ApprovedDate = DateTime.Now;
            project.AdminComment = null;

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject([FromBody] RejectModel model)
        {
            var project = await _context.Projects.FindAsync(model.Id);
            if (project == null)
                return Json(new { success = false, message = "Project not found." });

            if (string.IsNullOrWhiteSpace(model.Reason))
                return Json(new { success = false, message = "Rejection reason is required." });

            project.Status = "Rejected";
            project.AdminComment = model.Reason;
            project.ApprovedDate = null;

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        public async Task<IActionResult> Details(int id)
        {
            var project = await _context.Projects
        .Include(p => p.Company)
        .Include(p => p.ProjectType)
        .Include(p => p.Language)
        .Include(p => p.Framework)
        .Include(p => p.Files)
        .Include(p => p.ProjectMembers)
            .ThenInclude(pm => pm.Student) // Include Student data
        .FirstOrDefaultAsync(p => p.Project_pkId == id);

            if (project == null)
            {
                return NotFound();
            }

            ViewBag.IsPending = project.Status == "Pending";
            ViewBag.StatusMessage = TempData["StatusMessage"];
            ViewBag.IsSuccess = TempData["IsSuccess"];

            return View(project);
        }

        public class RejectModel
        {
            public int Id { get; set; }
            public string Reason { get; set; }
        }

    }
}
