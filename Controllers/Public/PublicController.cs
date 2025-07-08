using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.ViewModels;
using X.PagedList;




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
        public IActionResult ProjectIdeas(int? projectTypeId, int page = 1)
        {
            int pageSize = 4;

            var query = _context.Projects
                .Include(p => p.ProjectType)
                .Include(p => p.Files)
                .AsQueryable();

            if (projectTypeId.HasValue)
            {
                query = query.Where(p => p.ProjectType_pkId == projectTypeId.Value);
            }

            var totalProjects = query.Count();

            var projects = query
                .OrderByDescending(p => p.Project_pkId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new ProjectIdeasViewModel
            {
                Projects = projects,
                CurrentPage = page,
                PageSize = pageSize,
                TotalProjects = totalProjects,
                SelectedProjectTypeId = projectTypeId,
                ProjectTypes = _context.ProjectTypes
                    .Select(pt => new SelectListItem { Value = pt.ProjectType_pkId.ToString(), Text = pt.TypeName })
                    .ToList()
            };

            return View(model);
        }
        public IActionResult Index()
        {
            var viewModel = new DashboardViewModel
            {
                ProjectCount = _context.Projects.Count(),
                CompanyCount = _context.Companies.Count(),
                CityCount = _context.Cities.Count(),

                RecentCompanies = _context.Companies
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(5)
                    .Select(c => new RecentCompanyViewModel
                    {
                        CompanyName = c.CompanyName,
                        CreatedAt = c.CreatedAt
                    })
                    .ToList()
            };

            return View(viewModel);
        }




        public async Task<IActionResult> SuccessStories()
        {
            var stories = await _context.SuccessStories.ToListAsync();
            return View(stories);
        }

        // Form to submit story
        [HttpGet]
        public IActionResult SubmitSuccessStory()
        {
            return View();
        }

        // Handle form post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitSuccessStory(SuccessStory model)
        {
            if (ModelState.IsValid)
                return View(model);

            if (model.AccessCode != AccessCode)
            {
                ModelState.AddModelError("AccessCode", "Invalid access code.");
                return View(model);
            }

            // Save uploaded image
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", _imageFolder);
                if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);

                var fileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
                var filePath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                model.ImageFileName = fileName;
            }

            _context.SuccessStories.Add(model);
            await _context.SaveChangesAsync();

            //TempData["Message"] = "Your story has been submitted!";
            return RedirectToAction("SuccessStories");
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

            var allCities = await _context.Cities.OrderBy(c => c.CityName).ToListAsync();
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
                TotalCities = allCities.Count,
                Cities = pagedCities,
                CurrentPage = page
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



    }
}
