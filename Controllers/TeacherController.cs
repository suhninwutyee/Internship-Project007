using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.ViewModels;
using Microsoft.AspNetCore.Diagnostics;

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

                    //Announcements = await GetRecentAnnouncementsAsync(),

                    RecentSubmitters = await GetRecentSubmittersAsync(),

                    TotalProjects = await _context.Projects
                        .Where(p => p.IsDeleted == null || !p.IsDeleted.Value)
                        .CountAsync(),

                    SubmissionStats = submissionStats ?? new List<SubmissionStat>()
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

        private async Task<List<SubmissionStat>> GetSubmissionStatsAsync(DateTime currentDate)
        {
            try
            {
                var stats = Enumerable.Range(0, 7)
                    .Reverse()
                    .Select(i => new
                    {
                        Date = currentDate.AddDays(-i),
                        Count = 0
                    })
                    .ToList();

                var actualCounts = await _context.Projects
                    .Where(p => p.ProjectSubmittedDate.HasValue &&
                           p.ProjectSubmittedDate.Value.Date >= currentDate.AddDays(-6))
                    .GroupBy(p => p.ProjectSubmittedDate.Value.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();

                return stats.Select(s => new SubmissionStat
                {
                    Date = s.Date.ToString("MMM dd"),
                    Count = actualCounts.FirstOrDefault(a => a.Date == s.Date)?.Count ?? 0
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting submission stats");
                return new List<SubmissionStat>();
            }
        }

        //private async Task<List<Announcement>> GetRecentAnnouncementsAsync()
        //{
        //    try
        //    {
        //        return await _context.Announcements
        //            .OrderByDescending(a => a.CreatedDate)
        //            .Take(3)
        //            .ToListAsync() ?? new List<Announcement>();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error getting recent announcements");
        //        return new List<Announcement>();
        //    }
        //}

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
                    .ToListAsync() ?? new List<StudentSubmission>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent submitters");
                return new List<StudentSubmission>();
            }
        }

        public async Task<IActionResult> AllProjects()
        {
            try
            {
                var projects = await _context.Projects
                    .Include(p => p.Company)
                    .Include(p => p.ProjectType)
                    .Where(p => p.IsDeleted == null || !p.IsDeleted.Value)
                    .OrderByDescending(p => p.ProjectSubmittedDate)
                    .ToListAsync();

                return View("~/Views/ProjectApproval/Index.cshtml", new ProjectApprovalViewModel
                {
                    Projects = projects ?? new List<Project>(),
                    StatusFilter = "all",
                    PageTitle = "All Projects"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all projects");
                TempData["ErrorMessage"] = "Error loading projects";
                return RedirectToAction("Dashboard");
            }
        }

        public async Task<IActionResult> PendingProjects()
        {
            try
            {
                var projects = await _context.Projects
                    .Include(p => p.Company)
                    .Include(p => p.ProjectType)
                    .Where(p => p.Status == "Pending" && (p.IsDeleted == null || !p.IsDeleted.Value))
                    .OrderByDescending(p => p.ProjectSubmittedDate)
                    .ToListAsync();

                return View("~/Views/ProjectApproval/Index.cshtml", new ProjectApprovalViewModel
                {
                    Projects = projects ?? new List<Project>(),
                    StatusFilter = "Pending",
                    PageTitle = "Pending Projects"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending projects");
                TempData["ErrorMessage"] = "Error loading pending projects";
                return RedirectToAction("Dashboard");
            }
        }

        public async Task<IActionResult> ProjectsByDate(DateTime? date, string dateString = null)
        {
            try
            {
                DateTime filterDate = DateTime.Today;

                if (date.HasValue)
                {
                    filterDate = date.Value;
                }
                else if (!string.IsNullOrEmpty(dateString) && DateTime.TryParse(dateString, out var parsedDate))
                {
                    filterDate = parsedDate;
                }

                var projects = await _context.Projects
                    .Include(p => p.Company)
                    .Include(p => p.ProjectType)
                    .Where(p => p.ProjectSubmittedDate.HasValue &&
                           p.ProjectSubmittedDate.Value.Date == filterDate.Date &&
                           (p.IsDeleted == null || !p.IsDeleted.Value))
                    .OrderByDescending(p => p.ProjectSubmittedDate)
                    .ToListAsync();

                return View("~/Views/ProjectApproval/Index.cshtml", new ProjectApprovalViewModel
                {
                    Projects = projects ?? new List<Project>(),
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            _logger.LogError(exceptionHandlerPathFeature?.Error, "Error occurred in TeacherController");

            return View(new ErrorViewModel
            {
                RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}