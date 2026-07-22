using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using trix_site.Models;
using trix_site.Services;

namespace trix_site.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMarketingContactService _contactService;

        public HomeController(IMarketingContactService contactService)
        {
            _contactService = contactService;
        }

        // ---------- Static marketing pages ----------
        // עמוד הבית / הרשמה למשפחות
        public IActionResult Index() => View();

        [HttpGet("/Method")]
        public IActionResult Method() => View();

        [HttpGet("/Youth")]
        public IActionResult Youth() => View();

        [HttpGet("/Tutors")]
        public IActionResult Tutors() => View();

        [HttpGet("/Contact")]
        public IActionResult Contact() => View(new ContactViewModel());

        [HttpGet("/About")]
        public IActionResult About() => View();

        [HttpGet("/Terms")]
        public IActionResult Terms() => View();


        [HttpGet("/Privacy")]
        public IActionResult Privacy() => View();

        // עמוד רישום (GET) — נשאר כפי שהיה
        [HttpGet]
        public IActionResult Registration()
        {
            return View(new RegistrationViewModel());
        }

        // עמוד צור קשר (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactViewModel model)
        {
            model.Source = "צור קשר";

            // Honeypot: if the hidden field is filled, silently treat as success (bot).
            if (!string.IsNullOrWhiteSpace(model.Website))
                return RedirectToAction(nameof(ContactThanks));

            if (!ModelState.IsValid)
                return View(model);

            model.Language = "עברית";
            var ok = await _contactService.HandleContactAsync(model);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty,
                    "אירעה תקלה בשליחה. אפשר לפנות ישירות בוואטסאפ או בטלפון.");
                return View(model);
            }

            return RedirectToAction(nameof(ContactThanks));
        }

        [HttpGet]
        public IActionResult ContactThanks() => View();

        // ---------- Youth join form (POST) ----------
        // The Youth page (GET) is served by Youth() above; the form posts here.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YouthJoin(ContactViewModel model)
        {
            model.Source = "מובילי שינוי";

            if (!string.IsNullOrWhiteSpace(model.Website))
                return RedirectToAction(nameof(ContactThanks));

            if (!ModelState.IsValid)
                return View("Youth", model);

            model.Language = "עברית";
            var ok = await _contactService.HandleContactAsync(model);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty,
                    "אירעה תקלה בשליחה. אפשר לפנות ישירות בוואטסאפ או בטלפון.");
                return View("Youth", model);
            }

            return RedirectToAction(nameof(ContactThanks));
        }
    }
}
