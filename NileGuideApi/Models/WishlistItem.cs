namespace NileGuideApi.Models
{
    // Saved activity entry for a specific authenticated user.
    public class WishlistItem
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public int ActivityID { get; set; }
        public Activity? Activity { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
