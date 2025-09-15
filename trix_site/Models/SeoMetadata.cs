using System.ComponentModel.DataAnnotations;

namespace trix_site.Models
{
    public class SeoMetadata
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string? CanonicalUrl { get; set; }
        public string? OgImage { get; set; }
        public bool NoIndex { get; set; } = false;
        public string? OgType { get; set; } = "website";
        public string? TwitterCard { get; set; } = "summary_large_image";
        public string? Locale { get; set; } = "he_IL";
    }
}
