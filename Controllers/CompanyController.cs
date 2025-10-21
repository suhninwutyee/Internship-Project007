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
        private readonly PMSDbContext _context;

        public CompanyController(PMSDbContext context)
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
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            if (await _context.Companies.AnyAsync(c => c.CompanyName.ToLower() == model.CompanyName.Trim().ToLower()))
            {
                return Json(new { success = false, message = $"Company '{model.CompanyName}' already exists" });
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

                return Json(new
                {
                    success = true,
                    message = "Company created successfully",
                    data = new
                    {
                        company.Company_pkId,
                        company.CompanyName
                    }
                });
            }
            catch (DbUpdateException ex)
            {
                return Json(new { success = false, message = $"Error saving to database: {ex.InnerException?.Message ?? ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CompanyNameModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            var company = await _context.Companies.FindAsync(model.Company_pkId);
            if (company == null)
            {
                return Json(new { success = false, message = "Company not found" });
            }

            if (await _context.Companies.AnyAsync(c =>
                c.CompanyName.ToLower() == model.CompanyName.Trim().ToLower()
                && c.Company_pkId != model.Company_pkId))
            {
                return Json(new { success = false, message = $"Company '{model.CompanyName}' already exists" });
            }

            company.CompanyName = model.CompanyName.Trim();

            try
            {
                _context.Update(company);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Company updated successfully",
                    data = new
                    {
                        company.Company_pkId,
                        company.CompanyName
                    }
                });
            }
            catch (DbUpdateException ex)
            {
                return Json(new { success = false, message = $"Error saving to database: {ex.InnerException?.Message ?? ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return Json(new { success = false, message = "Company not found" });
            }

            try
            {
                _context.Companies.Remove(company);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Company deleted successfully",
                    data = new { id }
                });
            }
            catch (DbUpdateException ex)
            {
                return Json(new { success = false, message = $"Error deleting company: {ex.InnerException?.Message ?? ex.Message}" });
            }
        }
    }
}
