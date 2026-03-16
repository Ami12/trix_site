using Microsoft.AspNetCore.Mvc;
using trix_site.Models; // החלף בשם הפרויקט שלך
namespace trix_site.Controllers
{
    public class HomeController : Controller
    {
        // עמוד הבית
        public IActionResult Index() => View();

        // עמוד צור קשר
        public IActionResult Contact() => View();

        // עמוד תנאי שימוש
        public IActionResult Terms() => View();

        // עמוד מדיניות פרטיות
        public IActionResult Privacy() => View();

        // עמוד רישום (GET)
        [HttpGet]
        public IActionResult Registration()
        {
            return View(new RegistrationViewModel());
        }

        // עיבוד הרישום (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProcessRegistration(RegistrationViewModel model)
        {
            if (!ModelState.IsValid || !model.TermsAccepted || !model.ParentCommitment)
            {
                TempData["ErrorMessage"] = "יש לאשר את כל התנאים והמחויבות האישית כדי להמשיך.";
                return View("Registration", model);
            }

            // לינק לסליקה (960 ש"ח לחודש X 6 חודשים)
            string paymentUrl = "https://your-payment-link.co.il";
            return Redirect(paymentUrl);
        }
    }
}
