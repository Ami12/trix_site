using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using trix_site.Services;

namespace trix_site.Filters
{
    public class SeoMetadataFilter : IAsyncResultFilter
    {
        private readonly ISeoMetadataProvider _provider;

        public SeoMetadataFilter(ISeoMetadataProvider provider)
        {
            _provider = provider;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is ViewResult vr)
            {
                var meta = _provider.GetFor(context.HttpContext);
                vr.ViewData["Seo.Title"] = meta.Title;
                vr.ViewData["Seo.Description"] = meta.Description;
                vr.ViewData["Seo.Canonical"] = meta.CanonicalUrl;
                vr.ViewData["Seo.OgImage"] = meta.OgImage;
                vr.ViewData["Seo.NoIndex"] = meta.NoIndex ? "noindex,nofollow" : null;
                vr.ViewData["Seo.OgType"] = meta.OgType;
                vr.ViewData["Seo.Locale"] = meta.Locale;
                vr.ViewData["Seo.TwitterCard"] = meta.TwitterCard;
            }

            await next();
        }
    }
}
