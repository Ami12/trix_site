using trix_site.Models;

namespace trix_site.Services
{
    public interface ISeoMetadataProvider
    {
        SeoMetadata GetFor(HttpContext http);
    }
}
