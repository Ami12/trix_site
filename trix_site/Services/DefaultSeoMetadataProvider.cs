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

            _byPath = new(StringComparer.OrdinalIgnoreCase);

            // בית
            Add(new[] { "/", "/home", "/home/index" },
                new SeoMetadata
                {
                    Title = "12trix — מתמטיקה שנולדה מסקרנות",
                    Description = "קפיצה מדידה בהישגי מתמטיקה בלי להכביד על הצוות. למידה חווייתית, מדידה לפני/אחרי ותוצאות מוכחות.",
                    CanonicalUrl = $"{_siteBase}/",
                    OgImage = $"{_siteBase}/images/og/home.jpg"
                });

            // הורים
            Add(new[] { "/home/parents", "/parents" },
                new SeoMetadata
                {
                    Title = "להורים — 12trix",
                    Description = "אפליקציות מתמטיקה חווייתיות לילדים בכיתות ג־ו, תרגול מותאם ותגמולים שמייצרים ביטחון והצלחה.",
                    CanonicalUrl = $"{_siteBase}/Home/Parents",
                    OgImage = $"{_siteBase}/images/og/parents.jpg"
                });

            // בתי ספר
            Add(new[] { "/home/schools", "/schools" },
                new SeoMetadata
                {
                    Title = "לבתי ספר — 12trix",
                    Description = "שבוע נושא, קפיצת פתיחה לחטיבה ועוד — מדידה לפני/אחרי ויישום קל לצוות. תוצאות מוכחות.",
                    CanonicalUrl = $"{_siteBase}/Home/Schools",
                    OgImage = $"{_siteBase}/images/og/schools.jpg"
                });

            // אפליקציות
            Add(new[] { "/home/apps", "/apps" },
                new SeoMetadata
                {
                    Title = "האפליקציות שלנו — 12trix",
                    Description = "שברים, מאמן אריתמטי, לוח המאה, גיאומטריה ועוד — אפליקציות אינטראקטיביות שפועלות בדפדפן, בלי התקנה.",
                    CanonicalUrl = $"{_siteBase}/Home/Apps",
                    OgImage = $"{_siteBase}/images/og/apps.jpg"
                });

            // יצירת קשר
            Add(new[] { "/home/contact", "/contact" },
                new SeoMetadata
                {
                    Title = "יצירת קשר — 12trix",
                    Description = "צרו קשר להדגמה קצרה ולקבלת פרטים על תוכניות 12trix.",
                    CanonicalUrl = $"{_siteBase}/Home/Contact",
                    OgImage = $"{_siteBase}/images/og/contact.jpg"
                });
        }

        public SeoMetadata GetFor(HttpContext http)
        {
            var normalized = Normalize(http?.Request?.Path.Value);

            if (_byPath.TryGetValue(normalized, out var meta))
                return meta;

            // ברירת מחדל (לכל עמוד אחר)
            return new SeoMetadata
            {
                Title = "12trix",
                Description = "למידת מתמטיקה חווייתית שמובילה להצלחה.",
                CanonicalUrl = $"{_siteBase}{normalized}",
                OgImage = $"{_siteBase}/images/og/default.jpg"
            };
        }

        // ---------- Helpers ----------

        private void Add(string[] aliases, SeoMetadata meta)
        {
            foreach (var a in aliases)
            {
                _byPath[Normalize(a)] = meta;
            }
        }

        private static string Normalize(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return "/";

            var p = path.Trim().ToLowerInvariant();

            // וודא שמתחיל בסלאש
            if (!p.StartsWith("/")) p = "/" + p;

            // הסר סלאש סיום (מלבד root)
            if (p.Length > 1 && p.EndsWith("/"))
                p = p.TrimEnd('/');

            // נרמל הביתה
            if (p == "/home" || p == "/home/index")
                return "/";

            return p;
        }
    }
}
