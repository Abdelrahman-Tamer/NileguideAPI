namespace NileGuideApi.Models
{
    public class Review
    {
        public int ReviewId { get; set; }

        public int ActivityId { get; set; }
        public Activity Activity { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public string ReviewerName { get; set; } = string.Empty;
        public string? ReviewerCity { get; set; }

        public int Rating { get; set; }

        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}