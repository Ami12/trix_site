using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using trix_site.Models;

namespace trix_site.Controllers
{
    [Route("Apps")]
    public class AppsController : Controller
    {
        private readonly IWebHostEnvironment _env;
        public AppsController(IWebHostEnvironment env) { _env = env; }

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            var webroot = _env.WebRootPath;
            var jsonPath = Path.Combine(webroot, "data", "apps.json");

            if (!System.IO.File.Exists(jsonPath))
            {
                // אם אין JSON עדיין — נחזיר רשימה ריקה כדי שהעמוד יטען
                return View(new List<AppItem>());
            }

            var json = System.IO.File.ReadAllText(jsonPath);
            var items = JsonSerializer.Deserialize<List<AppItem>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<AppItem>();

            // שמירה על סדר הופעה מה-JSON
            return View(items);
        }
    }
}
