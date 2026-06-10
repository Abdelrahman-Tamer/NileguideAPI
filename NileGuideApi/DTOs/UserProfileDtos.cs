using System.ComponentModel.DataAnnotations;

namespace NileGuideApi.DTOs
{
    public class UserProfileResponseDto
    {
        public int UserId { get; set; }

        public string Email { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Nationality { get; set; } = string.Empty;

        public DateOnly DateOfBirth { get; set; }

        public int Age { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("profile_picture_url")]
        public string ProfilePictureUrl { get; set; } = string.Empty;

        public bool HasTravelDates { get; set; }

        public DateOnly? TravelStartDate { get; set; }

        public DateOnly? TravelEndDate { get; set; }

        public List<int> PreferredCityIds { get; set; } = new();

        public List<SelectedLookupDto> PreferredCities { get; set; } = new();

        public List<int> InterestCategoryIds { get; set; } = new();

        public List<SelectedLookupDto> Interests { get; set; } = new();

        public string Role { get; set; } = string.Empty;
    }

    public class SelectedLookupDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    public class UpdateUserProfileDto : IValidatableObject
    {
        // Optional in PUT. If sent, it updates Users.FullName.
        [MaxLength(150)]
        public string? FullName { get; set; }

        // Optional in PUT. If sent, it updates Users.Email.
        [EmailAddress(ErrorMessage = "Email is invalid")]
        [MaxLength(450)]
        public string? Email { get; set; }

        // Optional in PUT. If sent, it updates Users.DateOfBirth.
        public DateOnly? DateOfBirth { get; set; }

        // Optional in PUT. If sent, it updates Users.Nationality.
        [MaxLength(100)]
        public string? Nationality { get; set; }

        public bool? HasTravelDates { get; set; }

        public DateOnly? TravelStartDate { get; set; }

        public DateOnly? TravelEndDate { get; set; }

        public List<int>? PreferredCityIds { get; set; }

        public List<int>? InterestCategoryIds { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (FullName != null && string.IsNullOrWhiteSpace(FullName))
            {
                yield return new ValidationResult(
                    "FullName cannot be empty",
                    new[] { nameof(FullName) });
            }

            if (Email != null && string.IsNullOrWhiteSpace(Email))
            {
                yield return new ValidationResult(
                    "Email cannot be empty",
                    new[] { nameof(Email) });
            }

            if (DateOfBirth.HasValue)
            {
                if (DateOfBirth.Value == DateOnly.MinValue)
                {
                    yield return new ValidationResult(
                        "DateOfBirth is invalid",
                        new[] { nameof(DateOfBirth) });
                }

                if (DateOfBirth.Value >= today)
                {
                    yield return new ValidationResult(
                        "DateOfBirth must be in the past",
                        new[] { nameof(DateOfBirth) });
                }

                var age = CalculateAge(DateOfBirth.Value);

                if (age < 1 || age > 120)
                {
                    yield return new ValidationResult(
                        "Age must be between 1 and 120",
                        new[] { nameof(DateOfBirth) });
                }
            }

            if (Nationality != null && string.IsNullOrWhiteSpace(Nationality))
            {
                yield return new ValidationResult(
                    "Nationality cannot be empty",
                    new[] { nameof(Nationality) });
            }

            if (PreferredCityIds?.Any(id => id <= 0) == true)
            {
                yield return new ValidationResult(
                    "PreferredCityIds must contain positive values only",
                    new[] { nameof(PreferredCityIds) });
            }

            if (InterestCategoryIds?.Any(id => id <= 0) == true)
            {
                yield return new ValidationResult(
                    "InterestCategoryIds must contain positive values only",
                    new[] { nameof(InterestCategoryIds) });
            }

            if (TravelStartDate.HasValue &&
                TravelEndDate.HasValue &&
                TravelEndDate.Value < TravelStartDate.Value)
            {
                yield return new ValidationResult(
                    "TravelEndDate must be after or equal TravelStartDate",
                    new[] { nameof(TravelEndDate) });
            }
        }

        private static int CalculateAge(DateOnly dateOfBirth)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var age = today.Year - dateOfBirth.Year;

            if (dateOfBirth > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }
    }
}
