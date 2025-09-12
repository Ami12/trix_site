using System.Collections.Generic;

namespace trix_site.Models
{
    public class AppItem
    {
        public string Id { get; set; } = "";                  // מזהה ייחודי (למשל "equations")
        public string Title { get; set; } = "";
        public string? Subtitle { get; set; }                  // תיאור קצר בכרטיס
        public string Image { get; set; } = "";                // נתיב יחסי ל-wwwroot/images/apps/...
        public bool Soon { get; set; }                         // "בקרוב"
        public List<string> Meta { get; set; } = new();        // תגיות (תנאי מקדימים/שפה/פלטפורמה/בוטקמפ)
        public string? Price { get; set; }                     // טקסט מחיר (למשל "מחיר השקה: 120 ₪")
        public string? PriceStrike { get; set; }               // מחיר מלא (אם קיים)
        public string DetailsHtml { get; set; } = "";          // HTML שמופיע במודל
        public bool IncludedInBootcamp { get; set; }           // דגל "כלול בבוטקמפ" אם תרצה לשימוש עתידי
    }
}
