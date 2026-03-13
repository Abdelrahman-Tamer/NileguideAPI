namespace NileGuideApi.Models
{
    // Stores hashed reset-code state for the password recovery flow.
    public class PasswordResetToken
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        // The raw reset code is never stored, only its hashed representation.
        public string TokenHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }

        public DateTime? UsedAt { get; set; }

        // Failed attempts are tracked to stop brute-force guessing of reset codes.
        public int AttemptCount { get; set; } = 0;
        public DateTime? LastAttemptAt { get; set; }
    }
}
