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

        public bool HasTravelDates { get; set; }

        public DateOnly TravelStartDate { get; set; }

        public DateOnly TravelEndDate { get; set; }

        public string BudgetLevel { get; set; } = string.Empty;

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

        public bool HasTravelDates { get; set; }

        public DateOnly TravelStartDate { get; set; } = DateOnly.MinValue;

        public DateOnly TravelEndDate { get; set; } = DateOnly.MinValue;

        [Required(ErrorMessage = "BudgetLevel is required")]
        [MaxLength(50)]
        public string BudgetLevel { get; set; } = string.Empty;

        [Required(ErrorMessage = "PreferredCityIds is required")]
        public List<int> PreferredCityIds { get; set; } = new();

        [Required(ErrorMessage = "InterestCategoryIds is required")]
        public List<int> InterestCategoryIds { get; set; } = new();

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

            var allowedBudgets = new[] { "Economy", "Comfort", "Luxury" };

            if (!allowedBudgets.Contains(BudgetLevel?.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                yield return new ValidationResult(
                    "BudgetLevel must be Economy, Comfort, or Luxury",
                    new[] { nameof(BudgetLevel) });
            }

            if (PreferredCityIds == null || PreferredCityIds.Count == 0)
            {
                yield return new ValidationResult(
                    "PreferredCityIds must contain at least one city",
                    new[] { nameof(PreferredCityIds) });
            }

            if (InterestCategoryIds == null || InterestCategoryIds.Count == 0)
            {
                yield return new ValidationResult(
                    "InterestCategoryIds must contain at least one category",
                    new[] { nameof(InterestCategoryIds) });
            }

            if (HasTravelDates)
            {
                if (TravelStartDate == DateOnly.MinValue)
                {
                    yield return new ValidationResult(
                        "TravelStartDate is required when HasTravelDates is true",
                        new[] { nameof(TravelStartDate) });
                }

                if (TravelEndDate == DateOnly.MinValue)
                {
                    yield return new ValidationResult(
                        "TravelEndDate is required when HasTravelDates is true",
                        new[] { nameof(TravelEndDate) });
                }

                if (TravelEndDate < TravelStartDate)
                {
                    yield return new ValidationResult(
                        "TravelEndDate must be after or equal TravelStartDate",
                        new[] { nameof(TravelEndDate) });
                }
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