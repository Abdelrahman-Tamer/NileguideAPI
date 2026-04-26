using System.ComponentModel.DataAnnotations;

namespace NileGuideApi.DTOs
{
    // Request body for sending a newsletter blast to all active subscribers.
    public class SendNewsletterDto
    {
        [Required(ErrorMessage = "Subject is required")]
        [MaxLength(150, ErrorMessage = "Subject must be at most 150 characters")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Body is required")]
        [MaxLength(10000, ErrorMessage = "Body must be at most 10000 characters")]
        public string Body { get; set; } = string.Empty;
    }
}
