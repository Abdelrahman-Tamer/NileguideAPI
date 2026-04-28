using System.ComponentModel.DataAnnotations;

namespace NileGuideApi.DTOs
{
    /// <summary>
    /// Request body used by admins to send a newsletter campaign.
    /// </summary>
    public class SendNewsletterDto
    {
        /// <summary>
        /// Newsletter email subject.
        /// </summary>
        [Required(ErrorMessage = "Subject is required")]
        [MaxLength(150, ErrorMessage = "Subject must be at most 150 characters")]
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Newsletter body content.
        /// </summary>
        [Required(ErrorMessage = "Body is required")]
        [MaxLength(10000, ErrorMessage = "Body must be at most 10000 characters")]
        public string Body { get; set; } = string.Empty;
    }
}
