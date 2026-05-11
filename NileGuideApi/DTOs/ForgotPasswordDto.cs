using System.ComponentModel.DataAnnotations;

namespace NileGuideApi.DTOs
{
    /// <summary>
    /// Request body used to start the password reset process.
    /// </summary>
    public class ForgotPasswordDto
    {
        /// <summary>
        /// Account email address that should receive the reset code if it exists.
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
    }
}
