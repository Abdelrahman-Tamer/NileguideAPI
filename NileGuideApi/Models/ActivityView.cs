namespace NileGuideApi.Models
{
    public class ActivityView
    {
        public int Id { get; set; }

        public int ActivityId { get; set; }
        public Activity Activity { get; set; } = null!;

        public DateTime ViewedAt { get; set; }
    }
}