using Microsoft.AspNetCore.Mvc;

namespace trix_site.Controllers
{
    [Route("Parents")]
    public class ParentsController : Controller
    {
        [HttpGet("Programs/{grade:regex(^K(5|6|7)$)}")]
        public IActionResult Programs(string grade)
        {
            // יחפש View בשם Views/Schools/ProgramsK5.cshtml וכן הלאה
            return View($"Programs{grade}");
        }


        [HttpGet("Results/{grade:regex(^K(5|6|7)$)}")]
        public IActionResult Results(string grade)
        {
            // יחפש View בשם Views/Schools/ResultsK5.cshtml וכן הלאה
            return View($"Results{grade}");
        }
    }
}
