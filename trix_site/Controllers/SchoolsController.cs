using Microsoft.AspNetCore.Mvc;
using trix_site.Models;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Web; // אם אין – אפשר בלי, רק לא נשתמש בו

namespace trix_site.Controllers
{
    [Route("Schools")]
    public class SchoolsController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _cfg;
        private readonly ILogger<SchoolsController> _logger;

        public SchoolsController(IWebHostEnvironment env, IConfiguration cfg, ILogger<SchoolsController> logger)
        {
            _env = env;
            _cfg = cfg;
            _logger = logger;
        }

        [HttpPost("SubmitLead")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitLead([FromForm] LeadDto dto)
        {
            // Honeypot
            if (!string.IsNullOrEmpty(dto.Website))
                return BadRequest(new { ok = false, error = "spam" });

            if (!ModelState.IsValid)
                return BadRequest(new { ok = false, errors = ModelState });

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
            var ua = Request.Headers["User-Agent"].ToString() ?? "";
            var url = $"{Request.Scheme}://{Request.Host}{Request.PathBase}{Request.Path}";
            var when = DateTime.UtcNow;

            try
            {
                await SaveLeadToCsvAsync(dto, when, ip, ua);

                await TrySendLeadEmailAsync(dto, when, ip, ua, url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SubmitLead: failed to persist/notify");
                var isAjaxErr = string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest",
                                              StringComparison.OrdinalIgnoreCase);
                if (isAjaxErr)
                    return StatusCode(500, new { ok = false, error = "internal_error" });

                return View("Thanks");
            }

            var isAjax = string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest",
                                       StringComparison.OrdinalIgnoreCase);
            if (isAjax)
                return Json(new { ok = true });

            return View("Thanks");
        }


        [HttpGet("Thanks")]
        public IActionResult Thanks()
        {
            ViewData["Title"] = "תודה!";
            return View();
        }


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

        // ===== Helpers =====

        private async Task SaveLeadToCsvAsync(LeadDto dto, DateTime whenUtc, string ip, string ua)
        {
            // נתיב לשמירה: wwwroot/data/leads.csv (אפשר לשנות ב-appsettings)
            var defaultPath = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "data", "leads.csv");
            var path = _cfg["LeadSettings:StorePath"];
            if (string.IsNullOrWhiteSpace(path)) path = defaultPath;

            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            // כותרת אם הקובץ חדש
            var newFile = !System.IO.File.Exists(path);
            var sb = new StringBuilder();
            if (newFile)
            {
                sb.AppendLine(string.Join(",",
                    "when_utc", "cta_type", "section", "full_name", "role", "school_name",
                    "email", "phone", "notes", "classes", "topic", "grades", "teachers", "ip", "user_agent"));
            }

            string Q(string? s) => $"\"{(s ?? "").Replace("\"", "\"\"")}\"";
            sb.AppendLine(string.Join(",",
                Q(whenUtc.ToString("u")),
                Q(dto.Cta_Type),
                Q(dto.Section),
                Q(dto.Full_Name),
                Q(dto.Role),
                Q(dto.School_Name),
                Q(dto.Email),
                Q(dto.Phone),
                Q(dto.Notes),
                Q(dto.Classes?.ToString()),
                Q(dto.Topic),
                Q(dto.Grades),
                Q(dto.Teachers?.ToString()),
                Q(ip),
                Q(ua)
            ));

            await System.IO.File.AppendAllTextAsync(path, sb.ToString(), Encoding.UTF8);
        }

        private async Task TrySendLeadEmailAsync(LeadDto dto, DateTime whenUtc, string ip, string ua, string url)
        {
            // קורא הגדרות מייל מ-appsettings.json (ראה סעיף 2)
            var host = _cfg["Email:SmtpHost"];
            var port = _cfg.GetValue<int?>("Email:SmtpPort") ?? 587;
            var user = _cfg["Email:Username"];
            var pass = _cfg["Email:Password"];
            var ssl = _cfg.GetValue<bool?>("Email:EnableSsl") ?? true;

            var fromAddr = _cfg["LeadSettings:From"] ?? "support@12trix.com";
            var toAddr = _cfg["LeadSettings:NotifyTo"] ?? "ami@12trix.com";
            var ccAddr = _cfg["LeadSettings:NotifyCc"]; // אופציונלי

            // אם אין SMTP מוגדר – לא נעשה כלום
            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(toAddr))
                return;

            var subject = $"Lead חדש מאתר – {dto.Full_Name} ({dto.School_Name})";
            var body = $@"
נקלטה פנייה חדשה (UTC {whenUtc:u})

CTA: {dto.Cta_Type}    | Section: {dto.Section}
שם מלא: {dto.Full_Name}
תפקיד: {dto.Role}
בית ספר: {dto.School_Name}
דוא""ל: {dto.Email}
טלפון: {dto.Phone}
הערות: {dto.Notes}

תוספות:
מס' כיתות: {dto.Classes}
נושא: {dto.Topic}
שכבות: {dto.Grades}
מס' מורים: {dto.Teachers}

טכני:
IP: {ip}
User-Agent: {ua}
URL: {url}
";

            using var mail = new MailMessage();
            mail.From = new MailAddress(fromAddr, "12trix – Website");
            mail.To.Add(new MailAddress(toAddr));
            if (!string.IsNullOrWhiteSpace(ccAddr)) mail.CC.Add(new MailAddress(ccAddr));
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

   
}
