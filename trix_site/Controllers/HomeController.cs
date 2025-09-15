using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using trix_site.Models;

public class HomeController : Controller
{
    private readonly IWebHostEnvironment _env;
    public HomeController(IWebHostEnvironment env) { _env = env; }
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "12trix - בית";
        return View();
    }

    [HttpGet]
    public IActionResult Parents()
    {
        ViewData["Title"] = "12trix - הורים";
        return View();
    }

    [HttpGet]
    public IActionResult Schools()
    {
        ViewData["Title"] = "12trix - בתי ספר";
        return View();
    }

    [HttpGet]
    public IActionResult Contact()
    {
        ViewData["Title"] = "צור קשר";
        return View();
    }

    [HttpGet("Apps")]
    public IActionResult Apps()
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

    [Route("Home/Error")]
    public IActionResult Error(int? code = null)
    {
        ViewData["Title"] = "שגיאה";
        ViewBag.StatusCode = code;
        return View("~/Views/Shared/Error.cshtml");
    }
}
