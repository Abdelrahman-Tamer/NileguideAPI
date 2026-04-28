using System.ComponentModel.DataAnnotations;

namespace NileGuideApi.DTOs
{
    /// <summary>
    /// Request body used to subscribe to the newsletter.
    /// </summary>
    public class SubscribeNewsletterDto
    {
        /// <summary>
        /// Email address to subscribe.
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email format is invalid")]
        [MaxLength(320, ErrorMessage = "Email must be at most 320 characters")]
        public string Email { get; set; } = string.Empty;
    }
}
