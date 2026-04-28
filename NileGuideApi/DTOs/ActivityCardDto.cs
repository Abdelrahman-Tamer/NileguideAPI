namespace NileGuideApi.DTOs
{
    /// <summary>
    /// Compact activity data returned in paged activity lists.
    /// </summary>
    public class ActivityCardDto
    {
        /// <summary>
        /// Activity identifier.
        /// </summary>
        public int ActivityID { get; set; }

        /// <summary>
        /// Activity display name.
        /// </summary>
        public string ActivityName { get; set; } = string.Empty;

        /// <summary>
        /// Short activity description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Category identifier.
        /// </summary>
        public int CategoryID { get; set; }

        /// <summary>
        /// Category display name.
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>
        /// City identifier.
        /// </summary>
        public int CityID { get; set; }

        /// <summary>
        /// City display name.
        /// </summary>
        public string CityName { get; set; } = string.Empty;

        /// <summary>
        /// Lowest known price for the activity.
        /// </summary>
        public decimal MinPrice { get; set; }

        /// <summary>
        /// ISO-style display currency for activity prices.
        /// </summary>
        public string PriceCurrency { get; set; } = "USD";

        /// <summary>
        /// Primary image URL used by cards.
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// Documents required before booking or attending.
        /// </summary>
        public string RequiredDocuments { get; set; } = string.Empty;

        /// <summary>
        /// Whether the activity is currently active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Average rating for the activity.
        /// </summary>
        public double Rating { get; set; } = 5.0;

        /// <summary>
        /// Number of reviews for the activity.
        /// </summary>
        public int ReviewsCount { get; set; } = 89;

        /// <summary>
        /// Available external booking providers.
        /// </summary>
        public List<ActivityProviderDto> Providers { get; set; } = new();

        /// <summary>
        /// Opening hours associated with the activity.
        /// </summary>
        public List<ActivityHourDto> OpeningHours { get; set; } = new();
    }
}
