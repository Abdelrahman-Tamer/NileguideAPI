using System.Text.Json.Serialization;

namespace NileGuideApi.DTOs
{
    /// <summary>
    /// Standard response body for simple success or error messages.
    /// </summary>
    public class MessageResponseDto
    {
        /// <summary>
        /// Human-readable result message.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Standard response body returned when request validation fails.
    /// </summary>
    public class ValidationErrorResponseDto
    {
        /// <summary>
        /// General validation failure message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Validation errors keyed by request field name.
        /// </summary>
        public Dictionary<string, string> Errors { get; set; } = new();
    }

    /// <summary>
    /// Public profile data returned for the authenticated user.
    /// </summary>
    public class UserProfileDto
    {
        /// <summary>
        /// User identifier.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Normalized user email address.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User display name.
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// User nationality.
        /// </summary>
        public string Nationality { get; set; } = string.Empty;

        /// <summary>
        /// User date of birth, when provided.
        /// </summary>
        public DateOnly? DateOfBirth { get; set; }

        /// <summary>
        /// Current age calculated from DateOfBirth. Not stored in the database.
        /// </summary>
        public int? Age { get; set; }

        /// <summary>
        /// Public profile picture URL, when uploaded.
        /// </summary>
        [JsonPropertyName("profile_picture_url")]
        public string? ProfilePictureUrl { get; set; }

        /// <summary>
        /// Application role assigned to the user.
        /// </summary>
        public string Role { get; set; } = string.Empty;
    }

    public class ProfilePictureResponseDto
    {
        [JsonPropertyName("profile_picture_url")]
        public string ProfilePictureUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result returned after sending a newsletter campaign.
    /// </summary>
    public class NewsletterSendResultDto
    {
        /// <summary>
        /// Total active subscribers targeted by the campaign.
        /// </summary>
        public int TotalSubscribers { get; set; }

        /// <summary>
        /// Number of emails sent successfully.
        /// </summary>
        public int SentCount { get; set; }

        /// <summary>
        /// Number of emails that failed to send.
        /// </summary>
        public int FailedCount { get; set; }
    }
}
