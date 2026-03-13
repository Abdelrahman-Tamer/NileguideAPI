using System.ComponentModel.DataAnnotations;

namespace NileGuideApi.DTOs
{
    // Completes the password reset flow using the emailed code and a new password.
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email format is invalid")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Code is required")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Code must be 6 digits")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "NewPassword is required")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).{8,}$", ErrorMessage = "NewPassword must be at least 8 characters and include letters and numbers")]
        [MaxLength(100, ErrorMessage = "NewPassword must be at most 100 characters")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
