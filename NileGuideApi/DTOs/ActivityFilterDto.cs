using System.ComponentModel.DataAnnotations;

namespace NileGuideApi.DTOs
{
    /// <summary>
    /// Query-string filters used by the activity listing endpoint.
    /// </summary>
    public class ActivityFilterDto : IValidatableObject
    {
        private static readonly HashSet<string> AllowedSortValues = new(StringComparer.OrdinalIgnoreCase)
        {
            "default",
            "pricelowtohigh",
            "pricehightolow",
            "name"
        };

        /// <summary>
        /// Optional category ids used to filter activities.
        /// </summary>
        public List<int>? CategoryIds { get; set; }

        /// <summary>
        /// Optional city ids used to filter activities.
        /// </summary>
        public List<int>? CityIds { get; set; }

        /// <summary>
        /// Optional text search over activity name and description.
        /// </summary>
        [MaxLength(100, ErrorMessage = "Search must be at most 100 characters")]
        public string? Search { get; set; }

        /// <summary>
        /// Sort mode. Allowed values: default, priceLowToHigh, priceHighToLow, name.
        /// </summary>
        public string SortBy { get; set; } = "default";

        /// <summary>
        /// Page number starting from 1.
        /// </summary>
        [Range(1, 10000, ErrorMessage = "Page must be between 1 and 10000")]
        public int Page { get; set; } = 1;

        /// <summary>
        /// Number of activities per page.
        /// </summary>
        [Range(1, 50, ErrorMessage = "PageSize must be between 1 and 50")]
        public int PageSize { get; set; } = 9;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!AllowedSortValues.Contains(SortBy?.Trim() ?? string.Empty))
            {
                yield return new ValidationResult(
                    "SortBy must be one of: default, priceLowToHigh, priceHighToLow, name",
                    new[] { nameof(SortBy) });
            }

            if (CategoryIds is { Count: > 50 })
            {
                yield return new ValidationResult(
                    "CategoryIds must contain at most 50 values",
                    new[] { nameof(CategoryIds) });
            }

            if (CityIds is { Count: > 50 })
            {
                yield return new ValidationResult(
                    "CityIds must contain at most 50 values",
                    new[] { nameof(CityIds) });
            }

            if (CategoryIds?.Any(id => id <= 0) == true)
            {
                yield return new ValidationResult(
                    "CategoryIds must contain positive values only",
                    new[] { nameof(CategoryIds) });
            }

            if (CityIds?.Any(id => id <= 0) == true)
            {
                yield return new ValidationResult(
                    "CityIds must contain positive values only",
                    new[] { nameof(CityIds) });
            }
        }
    }
}
