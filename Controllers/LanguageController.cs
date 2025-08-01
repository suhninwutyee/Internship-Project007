using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using System.Threading.Tasks;
using System.Linq;
using X.PagedList;
namespace ProjectManagementSystem.Controllers
{
    public class LanguageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LanguageController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Language
        public async Task<IActionResult> Index(int? page)
        {
            var rollNumber = HttpContext.Session.GetString("RollNumber");
            if (string.IsNullOrEmpty(rollNumber))
            {
                return RedirectToAction("Login", "StudentLogin");
            }

            int pageSize = 3;               // Number of items per page
            int pageNumber = page ?? 1;     // Current page number (default 1)

            var languages = await _context.Languages
                                .OrderBy(l => l.Language_pkId)  // Order items (required)
                                .ToPagedListAsync(pageNumber, pageSize);

            return View(languages);
        }

        // GET: Language/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var language = await _context.Languages
                .FirstOrDefaultAsync(m => m.Language_pkId == id);

            if (language == null)
                return NotFound();

            return View(language);
        }

        // GET: Language/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Language/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Language language)
        {
            if (!ModelState.IsValid)
            {
                _context.Languages.Add(language);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(language);
        }

        // GET: Language/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var language = await _context.Languages.FindAsync(id);
            if (language == null) return NotFound();

            return View(language);
        }

        // POST: Language/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Language language)
        {
            if (id != language.Language_pkId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                try
                {
                    _context.Update(language);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Languages.Any(e => e.Language_pkId == language.Language_pkId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(language);
        }


        // GET: Language/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var language = await _context.Languages.FindAsync(id);
            if (language == null) return NotFound();

            return View(language);
        }

        // POST: Language/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var language = await _context.Languages.FindAsync(id);
            if (language != null)
            {
                _context.Languages.Remove(language);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", "Language"); // Redirect back to list
        }       
    }
}
