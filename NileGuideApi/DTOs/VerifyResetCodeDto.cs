using System.ComponentModel.DataAnnotations;

namespace NileGuideApi.DTOs
{
    // Checks whether a reset code is still valid before changing the password.
    public class VerifyResetCodeDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Code is required")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Code must be 6 digits")]
        public string Code { get; set; } = string.Empty;
    }
}
