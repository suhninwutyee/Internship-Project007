using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MimeKit.Text;
using MimeKit;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.ViewModels;
using X.PagedList;
using System.Net.Mail;
using MailKit.Net.Smtp;

namespace ProjectManagementSystem.Controllers.Public
{
    public class PublicController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string AccessCode = "OLDSTUDENT2025"; // your secret code
        private readonly string _imageFolder = "uploads/successstories";
        public PublicController(ApplicationDbContext context)
        {
            _context = context;
        }      
        public async Task<IActionResult> ProjectIdeas(int? projectTypeId, int? languageId, string? searchTerm, int page = 1)
        {
            var projectsQuery = _context.Projects
                .Include(p => p.ProjectType)
                .Include(p => p.Language)
                .Include(p => p.Files)
                .Where(p => (bool)!p.IsDeleted && p.Status == "Approved");

            if (projectTypeId.HasValue)
            {
                projectsQuery = projectsQuery.Where(p => p.ProjectType_pkId == projectTypeId);
            }

            if (languageId.HasValue)
            {
                projectsQuery = projectsQuery.Where(p => p.Language_pkId == languageId);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                projectsQuery = projectsQuery.Where(p =>
                    p.ProjectName.Contains(searchTerm) ||
                    p.Description.Contains(searchTerm));
            }

            var totalProjects = await projectsQuery.CountAsync();
            var pageSize = 6;

            var paginatedProjects = await projectsQuery
                .OrderByDescending(p => p.Project_pkId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new ProjectIdeasViewModel
            {
                Projects = paginatedProjects,
                ProjectTypes = await _context.ProjectTypes
                    .Select(pt => new SelectListItem
                    {
                        Value = pt.ProjectType_pkId.ToString(),
                        Text = pt.TypeName
                    }).ToListAsync(),

                Languages = projectTypeId.HasValue
                    ? await _context.Languages
                        .Where(l => l.ProjectType_pkId == projectTypeId)
                        .Select(l => new SelectListItem
                        {
                            Value = l.Language_pkId.ToString(),
                            Text = l.LanguageName
                        }).ToListAsync()
                    : new List<SelectListItem>(),

                SelectedProjectTypeId = projectTypeId,
                SelectedLanguageId = languageId,
                SearchTerm = searchTerm,
                TotalProjects = totalProjects,
                CurrentPage = page
            };

            return View(viewModel);
        }      

        [HttpGet]
        public async Task<IActionResult> SearchSuggestions(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<string>());

            var suggestions = await _context.Projects
                .Where(p => (bool)!p.IsDeleted && p.ProjectName.Contains(term))
                .OrderBy(p => p.ProjectName)
                .Select(p => p.ProjectName)
                .Distinct()
                .Take(10)
                .ToListAsync();

            return Json(suggestions);
        }


        [HttpGet]
        public async Task<IActionResult> GetLanguagesByProjectTypeUsedInProjects(int projectTypeId)
        {
            var languages = await _context.Languages
                .Where(l => l.ProjectType_pkId == projectTypeId)
                .Select(l => new
                {
                    value = l.Language_pkId,
                    text = l.LanguageName
                }).ToListAsync();

            return Json(languages);
        }                      
        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        // POST: /Public/Contact       
        public IActionResult Guidelines()
        {
            return View();
        }
        [AllowAnonymous]
        public IActionResult Help()
        {
            // Return Help view
            return View();
        }
        public async Task<IActionResult> InternshipCompanies(int? selectedCityId, int page = 1)
        {
            int pageSize = 5;

            // All cities that have at least one internship company
            var allCities = await _context.Cities
                .Where(c => _context.Companies.Any(co => co.City_pkId == c.City_pkId))
                .OrderBy(c => c.CityName)
                .ToListAsync();

            // Filter for display (if selected city is chosen)
            var filteredCities = selectedCityId.HasValue
                ? allCities.Where(c => c.City_pkId == selectedCityId.Value).ToList()
                : allCities;

            var pagedCities = filteredCities.ToPagedList(page, pageSize);

            var viewModel = new CityListViewModel
            {
                SelectedCityId = selectedCityId,
                CityList = allCities.Select(c => new SelectListItem
                {
                    Value = c.City_pkId.ToString(),
                    Text = c.CityName
                }),
                TotalCities = filteredCities.Count,
                Cities = pagedCities,
                CurrentPage = page,
                //TotalPages = pagedCities.PageCount
            };

            return View(viewModel);
        }



        // New action: list cities with images and Explore links
        public async Task<IActionResult> CompaniesByCity(int? id, int page = 1)
        {
            if (id == null) return NotFound();

            int pageSize = 3;

            var city = await _context.Cities
                .Include(c => c.Companies)
                .FirstOrDefaultAsync(c => c.City_pkId == id);

            if (city == null) return NotFound();

            var pagedCompanies = city.Companies.OrderBy(c => c.CompanyName).ToPagedList(page, pageSize);

            ViewBag.CityName = city.CityName;
            ViewBag.CityId = city.City_pkId;
            ViewBag.TotalCompanies = city.Companies.Count;

            return View(pagedCompanies);
        }

        public async Task<IActionResult> Index()
        {
            var projects = await _context.Projects
                .Include(p => p.Language)
                .Include(p => p.ProjectType)
                .ToListAsync();

            // Group languages by trimmed lowercase to avoid duplicates caused by casing/spaces
            var languageGroups = projects
                .Where(p => p.Language != null)
                .GroupBy(p => p.Language.LanguageName.Trim().ToLower())
                .Select(g => new
                {
                    LanguageName = Capitalize(g.Key),
                    Count = g.Count()
                })
                .ToList();

            // Group project types normally
            var projectTypeGroups = projects
                .Where(p => p.ProjectType != null)
                .GroupBy(p => p.ProjectType.TypeName)
                .Select(g => new
                {
                    TypeName = g.Key,
                    Count = g.Count()
                })
                .ToList();

            var viewModel = new DashboardViewModel
            {
                ProjectCount = projects.Count,

                LanguageCount = languageGroups.Count,
                LanguageNames = languageGroups.Select(x => x.LanguageName).ToList(),
                LanguageCounts = languageGroups.Select(x => x.Count).ToList(),

                ProjectTypeCount = projectTypeGroups.Count,
                ProjectTypeChartLabels = projectTypeGroups.Select(x => x.TypeName).ToList(),
                ProjectTypeChartValues = projectTypeGroups.Select(x => x.Count).ToList(),

                PopularProjects = projects
                    .OrderByDescending(p => p.Project_pkId)
                    .Take(6)
                    .Select(p => new ProjectIdea
                    {
                        Title = p.ProjectName,
                        ShortDescription = p.Description.Length > 100
                            ? p.Description.Substring(0, 100) + "..."
                            : p.Description,
                        FullDescription = p.Description
                    })
                    .ToList()
            };

            return View(viewModel);
        }

        // Helper method to capitalize first letter
        private static string Capitalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            return char.ToUpper(input[0]) + input.Substring(1);
        }

    }
}
