using System.ComponentModel.DataAnnotations;

namespace NileGuideApi.DTOs
{
    // Minimal request shape for newsletter unsubscribe requests.
    public class UnsubscribeNewsletterDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email format is invalid")]
        [MaxLength(320, ErrorMessage = "Email must be at most 320 characters")]
        public string Email { get; set; } = string.Empty;
    }
}
