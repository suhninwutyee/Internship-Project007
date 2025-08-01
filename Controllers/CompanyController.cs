using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementSystem.Controllers
{
    public class CompanyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CompanyController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var companies = await _context.Companies
                .OrderBy(c => c.CompanyName)
                .Select(c => new CompanyViewModel
                {
                    Company_pkId = c.Company_pkId,
                    CompanyName = c.CompanyName
                })
                .ToListAsync();

            return View(companies);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CompanyNameModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Companies.AnyAsync(c => c.CompanyName.ToLower() == model.CompanyName.Trim().ToLower()))
                {
                    TempData["ErrorMessage"] = $"Company '{model.CompanyName}' already exists";
                    return RedirectToAction(nameof(Index));
                }

                var company = new Company
                {
                    CompanyName = model.CompanyName.Trim(),
                    Address = "To be added",
                    Contact = "To be added",
                    Description = "",
                    ImageFileName = "default.png",
                    CreatedDate = DateTime.Now
                };

                try
                {
                    _context.Companies.Add(company);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Company created successfully";
                }
                catch (DbUpdateException ex)
                {
                    TempData["ErrorMessage"] = $"Error saving to database: {ex.InnerException?.Message ?? ex.Message}";
                }
            }
            else
            {
                TempData["ErrorMessage"] = string.Join(", ",
                    ModelState.Values.SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CompanyNameModel model)
        {
            if (ModelState.IsValid)
            {
                var company = await _context.Companies.FindAsync(model.Company_pkId);
                if (company == null)
                {
                    TempData["ErrorMessage"] = "Company not found";
                    return RedirectToAction(nameof(Index));
                }

                if (await _context.Companies.AnyAsync(c =>
                    c.CompanyName.ToLower() == model.CompanyName.Trim().ToLower()
                    && c.Company_pkId != model.Company_pkId))
                {
                    TempData["ErrorMessage"] = $"Company '{model.CompanyName}' already exists";
                    return RedirectToAction(nameof(Index));
                }

                company.CompanyName = model.CompanyName.Trim();

                try
                {
                    _context.Update(company);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Company updated successfully";
                }
                catch (DbUpdateException ex)
                {
                    TempData["ErrorMessage"] = $"Error saving to database: {ex.InnerException?.Message ?? ex.Message}";
                }
            }
            else
            {
                TempData["ErrorMessage"] = string.Join(", ",
                    ModelState.Values.SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                TempData["ErrorMessage"] = "Company not found";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Companies.Remove(company);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Company deleted successfully";
            }
            catch (DbUpdateException ex)
            {
                TempData["ErrorMessage"] = $"Error deleting company: {ex.InnerException?.Message ?? ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
