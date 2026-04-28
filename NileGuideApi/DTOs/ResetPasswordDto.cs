using System.ComponentModel.DataAnnotations;

namespace NileGuideApi.DTOs
{
    /// <summary>
    /// Request body used to complete the password reset process.
    /// </summary>
    public class ResetPasswordDto
    {
        /// <summary>
        /// Account email address used in the reset request.
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email format is invalid")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Six-digit reset code sent by email.
        /// </summary>
        [Required(ErrorMessage = "Code is required")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Code must be 6 digits")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// New password with at least 8 characters and at least one letter and one number.
        /// </summary>
        [Required(ErrorMessage = "NewPassword is required")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).{8,}$", ErrorMessage = "NewPassword must be at least 8 characters and include letters and numbers")]
        [MaxLength(100, ErrorMessage = "NewPassword must be at most 100 characters")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
