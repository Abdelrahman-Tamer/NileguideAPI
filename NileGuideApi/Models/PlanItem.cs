namespace NileGuideApi.Models
{
    // Scheduled activity entry for a specific authenticated user.
    public class PlanItem
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public int ActivityId { get; set; }
        public Activity? Activity { get; set; }

        public DateOnly ScheduledDate { get; set; }
        public TimeOnly StartTime { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
