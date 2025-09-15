using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using trix_site.Models;

namespace trix_site.Services
{
    public class DefaultSeoMetadataProvider : ISeoMetadataProvider
    {
        private readonly string _siteBase; // למשל "https://12trix.co.il"
        private readonly Dictionary<string, SeoMetadata> _byPath;

        public DefaultSeoMetadataProvider(string siteBase)
        {
            _siteBase = siteBase.TrimEnd('/');

            _byPath = new(StringComparer.OrdinalIgnoreCase)
            {
                ["/"] = new SeoMetadata
                {
                    Title = "12trix — מתמטיקה שנולדה מסקרנות",
                    Description = "קפיצה מדידה בהישגי מתמטיקה בלי להכביד על הצוות. למידה חווייתית, מדידה לפני/אחרי ותוצאות מוכחות.",
                    CanonicalUrl = $"{_siteBase}/",
                    OgImage = $"{_siteBase}/images/og/home.jpg"
                },
                ["/Parents"] = new SeoMetadata
                {
                    Title = "להורים — 12trix",
                    Description = "אפליקציות מתמטיקה חווייתיות לילדים בכיתות ג־ו, תרגול מותאם ותגמולים שמייצרים ביטחון והצלחה.",
                    CanonicalUrl = $"{_siteBase}/Parents",
                    OgImage = $"{_siteBase}/images/og/parents.jpg"
                },
                ["/Schools"] = new SeoMetadata
                {
                    Title = "לבתי ספר — 12trix",
                    Description = "שבוע נושא, קפיצת פתיחה לחטיבה ועוד תוכניות מוכחות— עם מדידה לפני/אחרי ויישום קל לצוות.",
                    CanonicalUrl = $"{_siteBase}/Schools",
                    OgImage = $"{_siteBase}/images/og/schools.jpg"
                },
                ["/Contact"] = new SeoMetadata
                {
                    Title = "יצירת קשר — 12trix",
                    Description = "צרו קשר להדגמה קצרה ולקבלת פרטים על תוכניות 12trix.",
                    CanonicalUrl = $"{_siteBase}/Contact",
                    OgImage = $"{_siteBase}/images/og/contact.jpg"
                },
                ["/Contact"] = new SeoMetadata
                {
                    Title = "יצירת קשר — 12trix",
                    Description = "צרו קשר להדגמה קצרה ולקבלת פרטים על תוכניות 12trix.",
                    CanonicalUrl = $"{_siteBase}/Contact",
                    OgImage = $"{_siteBase}/images/og/contact.jpg"
                },
            };
        }

        public SeoMetadata GetFor(HttpContext http)
        {
            var path = http.Request.Path.HasValue ? http.Request.Path.Value! : "/";
            if (_byPath.TryGetValue(path, out var meta)) return meta;

            // ברירת מחדל (לכל עמוד אחר)
            return new SeoMetadata
            {
                Title = "12trix",
                Description = "למידת מתמטיקה חווייתית שמובילה להצלחה.",
                CanonicalUrl = $"{_siteBase}{path}",
                OgImage = $"{_siteBase}/images/og/default.jpg"
            };
        }
    }
}
