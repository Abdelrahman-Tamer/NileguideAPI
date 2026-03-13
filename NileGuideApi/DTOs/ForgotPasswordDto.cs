using System.ComponentModel.DataAnnotations;

namespace NileGuideApi.DTOs
{
    // Starts the password reset process for an email address.
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
    }
}
