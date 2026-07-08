using System.ComponentModel.DataAnnotations;

namespace trix_site.Models
{
    /// <summary>
    /// Shared view model for the contact form (Contact page) and the
    /// youth "מובילי שינוי" join form. The Role field routes who is reaching out.
    /// </summary>
    public class ContactViewModel
    {
        /// <summary>
        /// Who is reaching out. Values used across the marketing site:
        /// parent, tutor, institution, teen, educator, other.
        /// The specific options shown depend on which page renders the form.
        /// </summary>
        [Required(ErrorMessage = "יש לבחור מי פונה")]
        public string Role { get; set; } = string.Empty;

        [Required(ErrorMessage = "יש להזין שם מלא")]
        [StringLength(100)]
        [Display(Name = "שם מלא")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "יש להזין מספר טלפון")]
        [Phone(ErrorMessage = "מספר טלפון לא תקין")]
        [Display(Name = "טלפון")]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "כתובת אימייל לא תקינה")]
        [Display(Name = "אימייל")]
        public string? Email { get; set; }

        [StringLength(2000)]
        [Display(Name = "הודעה")]
        public string? Message { get; set; }

        /// <summary>
        /// Identifies which page the submission came from (contact / youth),
        /// set by the controller action, useful for the notification email subject.
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Honeypot anti-spam field. Real users never fill it; bots often do.
        /// Must remain empty. Rendered hidden in the view.
        /// </summary>
        public string? Website { get; set; }

        public string? Language { get; set; }
    }
}
