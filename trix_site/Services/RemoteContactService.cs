using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using trix_site.Models;

namespace trix_site.Services
{
    public class RemoteContactService : IMarketingContactService
    {
        private readonly HttpClient _http;
        private readonly RemoteContactOptions _opts;
        private readonly ILogger<RemoteContactService> _logger;

        public RemoteContactService(
            HttpClient http,
            IOptions<RemoteContactOptions> opts,
            ILogger<RemoteContactService> logger)
        {
            _http = http;
            _opts = opts.Value;
            _logger = logger;
        }

        public async Task<bool> HandleContactAsync(ContactViewModel model)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, _opts.Endpoint);
                req.Headers.Add("X-Api-Key", "a3!2ttttAAA1X6a");
                req.Content = JsonContent.Create(model);

                var res = await _http.SendAsync(req);
                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogError("app.12trix.com returned {Status}", res.StatusCode);
                    return false;
                }
                return true;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to send contact to app.12trix.com");
                return false;
            }
        }
    }

    public class RemoteContactOptions
    {
        public string Endpoint { get; set; } = "https://app.12trix.com/api/marketing-contact";
        public string ApiKey { get; set; } = "a3!2ttttAAA1X6a";
    }
}