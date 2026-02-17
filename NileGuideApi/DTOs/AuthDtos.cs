using System.ComponentModel.DataAnnotations;

namespace NileGuideApi.DTOs
{
    public class RegisterDto
    {
        [Required, EmailAddress, MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6), MaxLength(100)]
        public string Password { get; set; } = string.Empty;

        [Required, MinLength(2), MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Nationality { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
