namespace NileGuideApi.Models
{
    // Application user used for authentication and profile data.
    public class User : AuditableEntity
    {
        public int Id { get; set; }

        // Email is the login identifier and is stored normalized in the controller.
        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Nationality { get; set; } = string.Empty;

        // Required because profile age is calculated from this value.
        public DateOnly DateOfBirth { get; set; }

        // Optional because user may not upload a profile picture.
        public string? ProfilePictureUrl { get; set; }

        // Role is currently used for authorization checks such as AdminOnly.
        public string Role { get; set; } = "Tourist";

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();

        public ICollection<PlanItem> PlanItems { get; set; } = new List<PlanItem>();

        public UserProfile? Profile { get; set; }
    }
}