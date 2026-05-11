using System.ComponentModel.DataAnnotations;

namespace NileGuideApi.DTOs
{
    /// <summary>
    /// Request body used to verify a password reset code before changing the password.
    /// </summary>
    public class VerifyResetCodeDto
    {
        /// <summary>
        /// Account email address used in the reset request.
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Six-digit reset code sent by email.
        /// </summary>
        [Required(ErrorMessage = "Code is required")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Code must be 6 digits")]
        public string Code { get; set; } = string.Empty;
    }
}
