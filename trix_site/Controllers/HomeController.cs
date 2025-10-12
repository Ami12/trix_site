using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Text.Json;
using trix_site.Models;
using Microsoft.Data.SqlClient;
using System.Numerics;
using System.Net.Mail;
public class HomeController : Controller
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _cfg;

    public HomeController(IWebHostEnvironment env, ILogger<HomeController> logger, IConfiguration cfg) 
    { 
        _env = env; 
        _logger = logger;
        _cfg = cfg;
    }


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
        return View("ParentsNew");
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ContactSubmit(string parentName, string phone, string childAge)
    {
        try
        {
            await TrySendLeadEmailAsync(parentName, phone, childAge);
            await TrySaveToDBAsync(parentName, phone, childAge);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SubmitLead: failed to persist/notify");
            return StatusCode(500, new { success = false, message = "Server error" });
        }
        return Json(new { success = true });
    }

    public async Task TrySaveToDBAsync(string parentName, string phone, string childAge)
    {
        const string sql = @"
                INSERT INTO ContactLeadsParentsIL
                (ParentName, Phone, ChildAge, CreatedAt)
                VALUES (@ParentName, @Phone, @ChildAge, SYSDATETIME());
            ";

        try
        {
            string connStr = _cfg.GetConnectionString("ApplicationDbContextConnection");
            using (var conn = new SqlConnection(connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                // הוספת פרמטרים עם הגנה מפני SQL Injection
                cmd.Parameters.AddWithValue("@ParentName", parentName);
                cmd.Parameters.AddWithValue("@Phone", phone);
                cmd.Parameters.AddWithValue("@ChildAge", childAge);
               
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }

            Console.WriteLine(" Contact form saved successfully.");
        }
        catch (SqlException ex)
        {
            // שגיאה שמגיעה ממסד הנתונים עצמו
            Console.WriteLine($" SQL Error: {ex.Message}");
            throw; // אפשר גם לבחור לא לזרוק הלאה
        }
        catch (Exception ex)
        {
            // כל שגיאה אחרת (בעיות חיבור, קונפיג, וכו')
            Console.WriteLine($" General DB Error: {ex.Message}");
            throw;
        }
    }

    private async Task TrySendLeadEmailAsync(string parentName, string phone, string childAge)
    {
        var host = _cfg["Email:SmtpHost"];
        var port = _cfg.GetValue<int?>("Email:SmtpPort") ?? 587;
        var user = _cfg["Email:Username"];
        var pass = _cfg["Email:Password"];
        var ssl = _cfg.GetValue<bool?>("Email:EnableSsl") ?? true;

        var fromAddr = _cfg["LeadSettings:From"] ?? "support@12trix.com";
        var toAddr = _cfg["LeadSettings:NotifyTo"] ?? "ami@12trix.com";


        // אם אין SMTP מוגדר – לא נעשה כלום
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(toAddr))
            return;

        var subject = $"קיבלנו ליד חדש מאתר .com";
        var body = $@"
            נקלטה פנייה חדשה (UTC {{DateTime.UtcNow:u}})

            שם ההורה: {{parentName}}
            טלפון: {{phone}}
            גיל הילד / כיתה: {{childAge}}

            ———————
            נשלח אוטומטית מדף ההורים באתר 12trix";

        using var mail = new MailMessage();
        mail.From = new MailAddress(fromAddr, "12trix – Website");
        mail.To.Add(new MailAddress(toAddr));
        mail.Subject = subject;
        mail.Body = body;
        mail.IsBodyHtml = false;

        using var smtp = new SmtpClient(host, port)
        {
            EnableSsl = ssl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass))
                          ? new System.Net.NetworkCredential(user, pass)
                          : System.Net.CredentialCache.DefaultNetworkCredentials
        };

        // שליחה
        await smtp.SendMailAsync(mail);
    }
}
