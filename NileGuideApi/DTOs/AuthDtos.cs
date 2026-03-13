using System.ComponentModel.DataAnnotations;

namespace NileGuideApi.DTOs
{
    // Request body used when creating a new user account.
    public class RegisterDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email format is invalid")]
        [MaxLength(255, ErrorMessage = "Email must be at most 255 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).{8,}$", ErrorMessage = "Password must be at least 8 characters and include letters and numbers")]
        [MaxLength(100, ErrorMessage = "Password must be at most 100 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "FullName is required")]
        [MinLength(2, ErrorMessage = "FullName must be at least 2 characters")]
        [MaxLength(150, ErrorMessage = "FullName must be at most 150 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nationality is required")]
        [MaxLength(100, ErrorMessage = "Nationality must be at most 100 characters")]
        public string Nationality { get; set; } = string.Empty;
    }

    // Request body used when logging in with email and password.
    public class LoginDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email format is invalid")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    // Request body used when rotating an access token with a valid refresh token.
    public class RefreshTokenDto
    {
        [Required(ErrorMessage = "RefreshToken is required")]
        public string RefreshToken { get; set; } = string.Empty;
    }

    // Shared auth response that preserves the legacy fields while extending the contract.
    public class AuthTokenResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
        public int UserId { get; set; }
        public string Role { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiresAtUtc { get; set; }
    }
}
