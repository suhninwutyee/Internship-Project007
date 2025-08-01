using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;

public class FrameworkController : Controller
{
    private readonly ApplicationDbContext _context;

    public FrameworkController(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<IActionResult> Index(int? page)
    {
        var rollNumber = HttpContext.Session.GetString("RollNumber");
        if (string.IsNullOrEmpty(rollNumber))
        {
            return RedirectToAction("Login", "StudentLogin");
        }

        int pageSize = 3;
        int pageNumber = page ?? 1;

        var frameworks = await _context.Frameworks
            .Include(f => f.Language)
            .OrderBy(f => f.Framework_pkId)
            .ToPagedListAsync(pageNumber, pageSize);

        return View(frameworks);
    }

    // GET: Framework/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var framework = await _context.Frameworks
            .Include(f => f.Language) // Include the related Language
            .FirstOrDefaultAsync(f => f.Framework_pkId == id);

        if (framework == null) return NotFound();

        return View(framework);
    }

    // GET: Framework/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var languages = await _context.Languages
            .Select(l => new SelectListItem
            {
                Value = l.Language_pkId.ToString(),
                Text = l.LanguageName
            })
            .ToListAsync();

        var model = new FrameworkCreateViewModel
        {
            Languages = languages
        };

        return View(model);
    }

    // POST: Framework/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FrameworkCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Reload Languages if validation fails
            model.Languages = await _context.Languages
                .Select(l => new SelectListItem
                {
                    Value = l.Language_pkId.ToString(),
                    Text = l.LanguageName
                })
                .ToListAsync();
            return View(model);
        }

        var framework = new Framework
        {
            FrameworkName = model.FrameworkName,
            Language_pkId = model.SelectedLanguageId
        };

        _context.Frameworks.Add(framework);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index)); // Make sure you have an Index action or redirect elsewhere
    }
    // GET: Framework/Edit/5
    // GET: Framework/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var framework = await _context.Frameworks.FindAsync(id);
        if (framework == null) return NotFound();

        var viewModel = new FrameworkCreateViewModel
        {
            FrameworkName = framework.FrameworkName,
            SelectedLanguageId = framework.Language_pkId,
            Languages = await _context.Languages
                .Select(l => new SelectListItem
                {
                    Value = l.Language_pkId.ToString(),
                    Text = l.LanguageName
                }).ToListAsync()
        };

        ViewBag.FrameworkId = framework.Framework_pkId;
        return View(viewModel);
    }

    // POST: Framework/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, FrameworkCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Reload dropdown if validation fails
            model.Languages = await _context.Languages
                .Select(l => new SelectListItem
                {
                    Value = l.Language_pkId.ToString(),
                    Text = l.LanguageName
                }).ToListAsync();

            ViewBag.FrameworkId = id;
            return View(model);
        }

        var framework = await _context.Frameworks.FindAsync(id);
        if (framework == null) return NotFound();

        framework.FrameworkName = model.FrameworkName;
        framework.Language_pkId = model.SelectedLanguageId;

        _context.Update(framework);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var framework = await _context.Frameworks
            .Include(f => f.Language)
            .FirstOrDefaultAsync(f => f.Framework_pkId == id);

        if (framework == null) return NotFound();

        return View(framework);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var framework = await _context.Frameworks.FindAsync(id);
        if (framework == null)
            return NotFound();

        _context.Frameworks.Remove(framework);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }



    // Optional: Index action to list frameworks

}
