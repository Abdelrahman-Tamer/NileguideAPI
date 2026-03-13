namespace NileGuideApi.Models
{
    // Represents a single newsletter subscription entry.
    public class NewsletterSubscriber
    {
        public int NewsletterID { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
