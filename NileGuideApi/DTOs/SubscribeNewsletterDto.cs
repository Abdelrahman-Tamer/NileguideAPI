using System.ComponentModel.DataAnnotations;

namespace NileGuideApi.DTOs
{
    // Minimal request shape for newsletter subscription.
    public class SubscribeNewsletterDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email format is invalid")]
        [MaxLength(320, ErrorMessage = "Email must be at most 320 characters")]
        public string Email { get; set; } = string.Empty;
    }
}
