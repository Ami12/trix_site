using System.ComponentModel.DataAnnotations;

namespace trix_site.Models
{
    public class LeadDto
    {
        // חבויים
        public string? Cta_Type { get; set; }
        public string? Section { get; set; }

        public string Full_Name { get; set; } = "";

        [MaxLength(120)]
        public string? Role { get; set; }

        public string? School_Name { get; set; }

        [EmailAddress, MaxLength(180)]
        public string? Email { get; set; }

        [MaxLength(40)]
        public string? Phone { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        // תוספות מותנות
        public int? Classes { get; set; }
        public string? Topic { get; set; }
        public string? Grades { get; set; }
        public int? Teachers { get; set; }

        // honeypot
        public string? Website { get; set; }
    }
}
