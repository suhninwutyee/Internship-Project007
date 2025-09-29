// Controllers/TeacherController.cs
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.ViewModels;
using System.Diagnostics;

namespace ProjectManagementSystem.Controllers
{
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TeacherController> _logger;

        public TeacherController(ApplicationDbContext context, ILogger<TeacherController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var date = DateTime.Now.Date;
                var submissionStats = await GetSubmissionStatsAsync(date);

                var model = new TeacherDashboardViewModel
                {
                    PendingProjectsCount = await _context.Projects
                        .Where(p => p.Status == "Pending" && (p.IsDeleted == null || !p.IsDeleted.Value))
                        .CountAsync(),

                    Announcements = await GetRecentAnnouncementsAsync(),

                    RecentSubmitters = await GetRecentSubmittersAsync(),

                    TotalProjects = await _context.Projects
                        .Where(p => p.IsDeleted == null || !p.IsDeleted.Value)
                        .CountAsync(),

                    SubmissionStats = submissionStats
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading teacher dashboard");
                TempData["ErrorMessage"] = "An error occurred while loading the dashboard.";
                return RedirectToAction("Error", "Home");
            }
        }

        private async Task<List<ViewModels.SubmissionStat>> GetSubmissionStatsAsync(DateTime currentDate)
        {
            try
            {
                var stats = new List<ViewModels.SubmissionStat>();
                var dateRange = Enumerable.Range(0, 7)
                    .Select(i => currentDate.AddDays(-i).Date)
                    .ToList();

                var actualCounts = await _context.Projects
                    .Where(p => p.ProjectSubmittedDate.HasValue &&
                           dateRange.Contains(p.ProjectSubmittedDate.Value.Date))
                    .GroupBy(p => p.ProjectSubmittedDate.Value.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();

                foreach (var date in dateRange.OrderBy(d => d))
                {
                    stats.Add(new ViewModels.SubmissionStat
                    {
                        Date = date.ToString("yyyy-MM-dd"),
                        Count = actualCounts.FirstOrDefault(a => a.Date == date)?.Count ?? 0
                    });
                }
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting submission stats");
                return Enumerable.Range(0, 7)
                    .Select(i => new ViewModels.SubmissionStat
                    {
                        Date = currentDate.AddDays(-i).ToString("yyyy-MM-dd"),
                        Count = 0
                    })
                    .ToList();
            }
        }

        private async Task<List<Announcement>> GetRecentAnnouncementsAsync()
        {
            try
            {
                return await _context.Announcements
                    .Where(a => a.IsActive) // Only get manually activated announcements
                    .OrderByDescending(a => a.CreatedDate)
                    .Take(1) // Only get the most recent active announcement
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent announcements");
                return new List<Announcement>();
            }
        }

        private async Task<List<StudentSubmission>> GetRecentSubmittersAsync()
        {
            try
            {
                return await _context.Projects
                    .Include(p => p.SubmittedByStudent)
                    .Where(p => p.Status == "Submitted" &&
                           p.SubmittedByStudent != null &&
                           p.ProjectSubmittedDate.HasValue)
                    .OrderByDescending(p => p.ProjectSubmittedDate)
                    .Take(5)
                    .Select(p => new StudentSubmission
                    {
                        StudentName = p.SubmittedByStudent.StudentName,
                        ProjectName = p.ProjectName,
                        SubmissionDate = p.ProjectSubmittedDate.Value
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent submitters");
                return new List<StudentSubmission>();
            }
        }

        public async Task<IActionResult> AllProjects()
        {
            return RedirectToAction("Index", "ProjectApproval", new { statusFilter = "all" });
        }

        public async Task<IActionResult> PendingProjects()
        {
            return RedirectToAction("Index", "ProjectApproval", new { statusFilter = "Pending" });
        }

        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            _logger.LogError(exceptionHandlerPathFeature?.Error, "Error occurred in TeacherController");

            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
        public async Task<IActionResult> ProjectsByDate(string date)
        {
            try
            {
                if (!DateTime.TryParse(date, out var filterDate))
                {
                    TempData["ErrorMessage"] = "Invalid date format";
                    return RedirectToAction("Dashboard");
                }

                var projects = await _context.Projects
                    .Include(p => p.Company)
                    .Include(p => p.ProjectType)
                    .Include(p => p.ProjectMembers)
                        .ThenInclude(pm => pm.Student)
                    .Where(p => p.ProjectSubmittedDate.HasValue &&
                           p.ProjectSubmittedDate.Value.Date == filterDate.Date &&
                           (p.IsDeleted == null || !p.IsDeleted.Value))
                    .OrderByDescending(p => p.ProjectSubmittedDate)
                    .ToListAsync();

                return View("~/Views/ProjectApproval/Index.cshtml", new ProjectApprovalViewModel
                {
                    Projects = projects,
                    PageTitle = $"Projects Submitted on {filterDate.ToString("MMM dd, yyyy")}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting projects by date");
                TempData["ErrorMessage"] = "Error loading projects by date";
                return RedirectToAction("Dashboard");
            }
        }
    }
}