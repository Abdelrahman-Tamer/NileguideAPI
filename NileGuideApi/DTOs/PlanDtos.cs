using System.ComponentModel.DataAnnotations;

namespace NileGuideApi.DTOs
{
    /// <summary>
    /// Request body used to add an activity to the authenticated user's plan.
    /// </summary>
    public class AddPlanItemRequest
    {
        /// <summary>
        /// Activity identifier to add to the plan.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Activity id must be positive")]
        public int ActivityId { get; set; }

        /// <summary>
        /// Date selected for the activity.
        /// </summary>
        public DateOnly ScheduledDate { get; set; }

        /// <summary>
        /// Start time in 24-hour HH:mm format.
        /// </summary>
        [Required]
        [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "StartTime must be in HH:mm format")]
        public string StartTime { get; set; } = string.Empty;
    }

    /// <summary>
    /// One scheduled activity row in the user's plan.
    /// </summary>
    public class PlanItemResponse
    {
        /// <summary>
        /// Plan item identifier.
        /// </summary>
        public int PlanItemId { get; set; }

        /// <summary>
        /// Activity identifier.
        /// </summary>
        public int ActivityId { get; set; }

        /// <summary>
        /// Date selected for the activity.
        /// </summary>
        public DateOnly ScheduledDate { get; set; }

        /// <summary>
        /// Start time in HH:mm format.
        /// </summary>
        public string StartTime { get; set; } = string.Empty;

        /// <summary>
        /// End time in HH:mm format, calculated from duration minutes.
        /// </summary>
        public string EndTime { get; set; } = string.Empty;

        /// <summary>
        /// Activity display name.
        /// </summary>
        public string ActivityName { get; set; } = string.Empty;

        /// <summary>
        /// Activity description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// City identifier.
        /// </summary>
        public int CityId { get; set; }

        /// <summary>
        /// City display name.
        /// </summary>
        public string CityName { get; set; } = string.Empty;

        /// <summary>
        /// Activity duration in minutes.
        /// </summary>
        public int DurationMinutes { get; set; }

        /// <summary>
        /// Activity price used for plan totals.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Activity price currency.
        /// </summary>
        public string PriceCurrency { get; set; } = "USD";

        /// <summary>
        /// Whether the activity is currently active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Available booking links for the activity.
        /// </summary>
        public List<BookingLinkDto> BookingLinks { get; set; } = new();
    }

    /// <summary>
    /// External booking provider link for a planned activity.
    /// </summary>
    public class BookingLinkDto
    {
        /// <summary>
        /// Provider display name.
        /// </summary>
        public string ProviderName { get; set; } = string.Empty;

        /// <summary>
        /// External booking URL.
        /// </summary>
        public string Link { get; set; } = string.Empty;
    }

    /// <summary>
    /// Authenticated user's activity plan.
    /// </summary>
    public class PlanResponse
    {
        /// <summary>
        /// Number of scheduled activities.
        /// </summary>
        public int TotalActivities { get; set; }

        /// <summary>
        /// Total cost calculated from activity prices.
        /// </summary>
        public decimal TotalCost { get; set; }

        /// <summary>
        /// Currency used for the plan total.
        /// </summary>
        public string PriceCurrency { get; set; } = "USD";

        /// <summary>
        /// Scheduled activity rows.
        /// </summary>
        public List<PlanItemResponse> Items { get; set; } = new();
    }
}
