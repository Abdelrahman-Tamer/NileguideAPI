using System.ComponentModel.DataAnnotations;

namespace NileGuideApi.DTOs
{
    public class ResetPasswordDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6), MaxLength(6)]
        public string Code { get; set; } = string.Empty;

        [Required, MinLength(8), MaxLength(100)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
