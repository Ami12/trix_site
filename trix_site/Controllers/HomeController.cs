using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
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

    [Route("Home/Error")]
    public IActionResult Error(int? code = null)
    {
        ViewData["Title"] = "שגיאה";
        ViewBag.StatusCode = code;
        return View("~/Views/Shared/Error.cshtml");
    }
}
