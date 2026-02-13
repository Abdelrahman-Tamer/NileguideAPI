namespace NileGuideApi.Models
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        public int? UserId { get; set; }  // nullable
        public User? User { get; set; }

        public string TokenHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public DateTime? UsedAt { get; set; }
        public int AttemptCount { get; set; } = 0;
        public DateTime? LastAttemptAt { get; set; }
    }
}