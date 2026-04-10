namespace NileGuideApi.Models
{
    // Stores ordered images associated with a specific activity.
    public class ActivityImage
    {
        public int ImageID { get; set; }

        public int ActivityID { get; set; }
        public Activity? Activity { get; set; }

        public string Url { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }
    }
}
