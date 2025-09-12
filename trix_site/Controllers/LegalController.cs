using Microsoft.AspNetCore.Mvc;

namespace trix_site.Controllers
{
    [Route("Legal")]
    public class LegalController : Controller
    {
        [HttpGet("Privacy")]
        public IActionResult Privacy() => View();

        [HttpGet("Terms")]
        public IActionResult Terms() => View();
    }
}
