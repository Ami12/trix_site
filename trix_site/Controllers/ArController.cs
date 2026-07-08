using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using trix_site.Models;
using trix_site.Services;

namespace trix_site.Controllers
{
    /// <summary>
    /// Arabic version of the marketing site, served under the /ar route prefix.
    /// Hebrew stays at the root (HomeController). Views live in Views/Home/ar/.
    ///
    /// NOTE: content is a best-effort Arabic draft and should be reviewed by a
    /// native speaker before going fully public.
    /// </summary>
    [Route("ar")]
    public class ArController : Controller
    {
        private readonly IMarketingContactService _contactService;

        public ArController(IMarketingContactService contactService)
        {
            _contactService = contactService;
        }

        // Helper: all Arabic views sit in Views/Home/ar/<name>.cshtml
        private ViewResult ArView(string name, object? model = null)
            => model == null ? View($"~/Views/Home/ar/{name}.cshtml")
                             : View($"~/Views/Home/ar/{name}.cshtml", model);

        // ---------- Pages ----------

        [HttpGet("")]              // /ar
        public IActionResult Index() => ArView("Index");

        [HttpGet("Method")]        // /ar/Method
        public IActionResult Method() => ArView("Method");

        [HttpGet("Youth")]         // /ar/Youth
        public IActionResult Youth() => ArView("Youth", new ContactViewModel { Language = "ar" });

        [HttpGet("Tutors")]        // /ar/Tutors
        public IActionResult Tutors() => ArView("Tutors");

        [HttpGet("Contact")]       // /ar/Contact
        public IActionResult Contact() => ArView("Contact", new ContactViewModel { Language = "ar" });

        // ---------- Contact form (POST) ----------

        [HttpPost("Contact")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactViewModel model)
        {
            model.Source = "צור קשר (ערבית)";
            model.Language = "ar";

            if (!string.IsNullOrWhiteSpace(model.Website))
                return RedirectToAction(nameof(ContactThanks));

            if (!ModelState.IsValid)
                return ArView("Contact", model);

            var ok = await _contactService.HandleContactAsync(model);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty,
                    "حدث خطأ في الإرسال. يمكنكم التواصل مباشرة عبر واتساب أو الهاتف.");
                return ArView("Contact", model);
            }

            return RedirectToAction(nameof(ContactThanks));
        }

        [HttpGet("ContactThanks")]
        public IActionResult ContactThanks() => ArView("ContactThanks");

        // ---------- Youth join form (POST) ----------

        [HttpPost("YouthJoin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YouthJoin(ContactViewModel model)
        {
            model.Source = "מובילי שינוי (ערבית)";
            model.Language = "ar";

            if (!string.IsNullOrWhiteSpace(model.Website))
                return RedirectToAction(nameof(ContactThanks));

            if (!ModelState.IsValid)
                return ArView("Youth", model);

            var ok = await _contactService.HandleContactAsync(model);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty,
                    "حدث خطأ في الإرسال. يمكنكم التواصل مباشرة عبر واتساب أو الهاتف.");
                return ArView("Youth", model);
            }

            return RedirectToAction(nameof(ContactThanks));
        }
    }
}
