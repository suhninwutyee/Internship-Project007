using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using System.Threading.Tasks;
using System.Linq;
using X.PagedList;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProjectManagementSystem.Controllers
{
    public class CompanyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly string _imagePath;

        public CompanyController(ApplicationDbContext context)
        {
            _context = context;
            _imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/companies");
        }

        // GET: Company        
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 3;
            var companies = await _context.Companies
                .Include(c => c.City)   // Include City navigation property
                .OrderBy(c => c.CompanyName)
                .ToPagedListAsync(page, pageSize);

            return View(companies);
        }

        // GET: Company/Create
        public IActionResult Create()
        {
            var cities = _context.Cities.OrderBy(c => c.CityName).ToList();

            // Cast City_pkId to string here to avoid cast issues
            ViewBag.CityList = new SelectList(
                cities.Select(c => new { Id = c.City_pkId.ToString(), c.CityName }),
                "Id",
                "CityName"
            );

            return View();
        }


        // POST: Company/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Company company, IFormFile imageFile)
        {
            
            if (!ModelState.IsValid)  // Fix here: must check IsValid, not !IsValid
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    if (!Directory.Exists(_imagePath))
                        Directory.CreateDirectory(_imagePath);

                    var fileName = Path.GetFileName(imageFile.FileName);
                    var filePath = Path.Combine(_imagePath, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await imageFile.CopyToAsync(stream);

                    company.ImageFileName = fileName;
                }

                _context.Add(company);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Reload city list if validation failed
            ViewBag.CityList = new SelectList(_context.Cities.OrderBy(c => c.CityName), "City_pkId", "CityName", company.City_pkId);
            return View(company);
        }

        // GET: Company/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var company = await _context.Companies.FindAsync(id);
            if (company == null) return NotFound();

            var cities = _context.Cities.OrderBy(c => c.CityName).ToList();

            ViewBag.CityList = new SelectList(
                cities.Select(c => new { Id = c.City_pkId.ToString(), c.CityName }),
                "Id",
                "CityName",
                company.City_pkId  // selected value as string
            );

            return View(company);
        }


        // POST: Company/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Company company, IFormFile imageFile)
        {
            company.City_pkId = int.Parse(company.City_pkId.ToString());
            Console.WriteLine("city pkid.........................." + company.City_pkId);
            Console.WriteLine("here edit post......................." + company.Company_pkId);
            if (company.Company_pkId != company.Company_pkId)
                return NotFound();

            if (!ModelState.IsValid)  // Fix here too
            {
                Console.WriteLine("here state valid.........................");
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        if (!Directory.Exists(_imagePath))
                            Directory.CreateDirectory(_imagePath);

                        var fileName = Path.GetFileName(imageFile.FileName);
                        var filePath = Path.Combine(_imagePath, fileName);

                        using var stream = new FileStream(filePath, FileMode.Create);
                        await imageFile.CopyToAsync(stream);

                        company.ImageFileName = fileName;
                    }
                    else
                    {
                        // Preserve existing image file name if no new upload
                        var existingCompany = await _context.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.Company_pkId == company.Company_pkId);
                        if (existingCompany != null)
                        {
                            company.ImageFileName = existingCompany.ImageFileName;
                        }
                    }

                    _context.Update(company);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Companies.Any(e => e.Company_pkId == company.Company_pkId))
                        return NotFound();
                    else
                        throw;
                }
            }

            ViewBag.CityList = new SelectList(_context.Cities.OrderBy(c => c.CityName), "City_pkId", "CityName", company.City_pkId);
            return View(company);
        }

        // GET: Company/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var company = await _context.Companies.Include(c => c.City).FirstOrDefaultAsync(m => m.Company_pkId == id);
            if (company == null) return NotFound();

            return View(company);
        }

        // POST: Company/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                _context.Companies.Remove(company);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
