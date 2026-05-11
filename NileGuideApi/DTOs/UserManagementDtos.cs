using System.ComponentModel.DataAnnotations;

namespace NileGuideApi.DTOs
{
    public class UserListItemDto
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public DateTime Joined { get; set; }

        public int WishlistItems { get; set; }

        public bool IsActive { get; set; }

        public string? ProfilePictureUrl { get; set; }
    }

    public class UserDetailsDto
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public DateTime Joined { get; set; }

        public DateTime? LastSeen { get; set; }

        public int WishlistItems { get; set; }

        public bool IsActive { get; set; }

        public string? ProfilePictureUrl { get; set; }

        public string Nationality { get; set; } = string.Empty;

        public DateOnly DateOfBirth { get; set; }
    }

    public class CreateUserDto : IValidatableObject
    {
        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Email is invalid")]
        [MaxLength(450)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Nationality { get; set; } = string.Empty;

        [Required]
        public DateOnly DateOfBirth { get; set; }

        [Required]
        public string Role { get; set; } = "User";

        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? ProfilePictureUrl { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(FullName))
            {
                yield return new ValidationResult(
                    "FullName is required",
                    new[] { nameof(FullName) });
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                yield return new ValidationResult(
                    "Email is required",
                    new[] { nameof(Email) });
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                yield return new ValidationResult(
                    "Password is required",
                    new[] { nameof(Password) });
            }

            if (string.IsNullOrWhiteSpace(Nationality))
            {
                yield return new ValidationResult(
                    "Nationality is required",
                    new[] { nameof(Nationality) });
            }

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
                age--;

            if (age < 1 || age > 120)
            {
                yield return new ValidationResult(
                    "Age must be between 1 and 120",
                    new[] { nameof(DateOfBirth) });
            }

            var normalizedRole = Role?.Trim();

            if (!string.Equals(normalizedRole, "Admin", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(normalizedRole, "User", StringComparison.OrdinalIgnoreCase))
            {
                yield return new ValidationResult(
                    "Role must be Admin or User",
                    new[] { nameof(Role) });
            }
        }
    }

    public class UpdateUserRoleDto : IValidatableObject
    {
        [Required]
        public string Role { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var normalizedRole = Role?.Trim();

            if (!string.Equals(normalizedRole, "Admin", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(normalizedRole, "User", StringComparison.OrdinalIgnoreCase))
            {
                yield return new ValidationResult(
                    "Role must be Admin or User",
                    new[] { nameof(Role) });
            }
        }
    }

    public class UpdateUserStatusDto
    {
        [Required]
        public bool IsActive { get; set; }
    }
}