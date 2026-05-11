using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NileGuideApi.DTOs
{
    /// <summary>
    /// Request body used to create a new tourist account.
    /// </summary>
    public class RegisterDto : IValidatableObject
    {
        /// <summary>
        /// User email address. It is normalized before storage and used for login.
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email format is invalid")]
        [MaxLength(255, ErrorMessage = "Email must be at most 255 characters")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Password with at least 8 characters and at least one letter and one number.
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).{8,}$", ErrorMessage = "Password must be at least 8 characters and include letters and numbers")]
        [MaxLength(100, ErrorMessage = "Password must be at most 100 characters")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// User full display name.
        /// </summary>
        [Required(ErrorMessage = "FullName is required")]
        [MinLength(2, ErrorMessage = "FullName must be at least 2 characters")]
        [MaxLength(150, ErrorMessage = "FullName must be at most 150 characters")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// User nationality.
        /// </summary>
        [Required(ErrorMessage = "Nationality is required")]
        [MaxLength(100, ErrorMessage = "Nationality must be at most 100 characters")]
        public string Nationality { get; set; } = string.Empty;

        /// <summary>
        /// Required user date of birth. Sent as YYYY-MM-DD and used to calculate age in responses.
        /// </summary>
        [DataType(DataType.Date)]
        public DateOnly DateOfBirth { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (DateOfBirth == DateOnly.MinValue)
            {
                yield return new ValidationResult(
                    "DateOfBirth is required",
                    new[] { nameof(DateOfBirth) });
            }

            if (DateOfBirth >= today)
            {
                yield return new ValidationResult(
                    "DateOfBirth must be in the past",
                    new[] { nameof(DateOfBirth) });
            }

            var age = today.Year - DateOfBirth.Year;

            if (DateOfBirth > today.AddYears(-age))
            {
                age--;
            }

            if (age < 1 || age > 120)
            {
                yield return new ValidationResult(
                    "Age must be between 1 and 120",
                    new[] { nameof(DateOfBirth) });
            }
        }
    }

    /// <summary>
    /// Request body used to log in with email and password.
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// Registered user email address.
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email format is invalid")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User password.
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Whether to issue a longer-lived refresh token.
        /// </summary>
        public bool RememberMe { get; set; }
    }

    /// <summary>
    /// Request body used to rotate an access token with a valid refresh token.
    /// </summary>
    public class RefreshTokenDto
    {
        /// <summary>
        /// Plain-text refresh token previously returned by login, register, or refresh.
        /// </summary>
        [Required(ErrorMessage = "RefreshToken is required")]
        public string RefreshToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// Authentication response returned by register, login, and refresh.
    /// </summary>
    public class AuthTokenResponseDto
    {
        /// <summary>
        /// JWT access token used as a bearer token.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// UTC expiry time for the access token.
        /// </summary>
        public DateTime ExpiresAtUtc { get; set; }

        /// <summary>
        /// Authenticated user identifier.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Application role assigned to the authenticated user.
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// User date of birth.
        /// </summary>
        public DateOnly DateOfBirth { get; set; }

        /// <summary>
        /// Current age calculated from DateOfBirth. Not stored in the database.
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Public profile picture URL, when uploaded.
        /// </summary>
        [JsonPropertyName("profile_picture_url")]
        public string? ProfilePictureUrl { get; set; }

        /// <summary>
        /// Plain-text refresh token. Store securely on the client.
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// UTC expiry time for the refresh token.
        /// </summary>
        public DateTime RefreshTokenExpiresAtUtc { get; set; }
    }
}