using System.ComponentModel.DataAnnotations;

namespace NileGuideApi.DTOs
{
    // Request body for sending a newsletter blast to all active subscribers.
    public class SendNewsletterDto
    {
        [Required(ErrorMessage = "Subject is required")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Body is required")]
        public string Body { get; set; } = string.Empty;
    }
}
