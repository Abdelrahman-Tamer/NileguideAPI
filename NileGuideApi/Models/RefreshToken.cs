namespace NileGuideApi.Models
{
    // Persisted refresh-token state used for rotation, revocation, and logout.
    public class RefreshToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? ReplacedByTokenHash { get; set; }
        public string? CreatedByIp { get; set; }
        public string? RevokedByIp { get; set; }
    }
}
