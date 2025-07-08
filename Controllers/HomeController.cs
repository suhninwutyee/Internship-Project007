using Microsoft.AspNetCore.Mvc;

namespace InternshipSystem.Controllers
{
    public class HomeController : Controller
    {
        // GET: /Home/Index
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Home/SubmitInterest (Optional - for form handling)
        [HttpPost]
        public IActionResult SubmitInterest(string email)
        {
            // Add logic to save email to DB or send notification
            TempData["Message"] = "Thanks for your interest! We'll contact you soon.";
            return RedirectToAction("Index");
        }
    }
}