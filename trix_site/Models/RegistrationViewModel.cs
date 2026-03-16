using System.ComponentModel.DataAnnotations;

namespace trix_site.Models
{
    public class RegistrationViewModel
    {
        [Required]
        public bool TermsAccepted { get; set; }

        [Required]
        public bool ParentCommitment { get; set; }
    }
}
