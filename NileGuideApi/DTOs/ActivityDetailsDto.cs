namespace NileGuideApi.DTOs
{
    /// <summary>
    /// Full activity details returned by the activity details endpoint.
    /// </summary>
    public class ActivityDetailsDto
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
        /// Full activity description.
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
        /// Activity latitude.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Activity longitude.
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Current listed price.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Lowest known price.
        /// </summary>
        public decimal MinPrice { get; set; }

        /// <summary>
        /// ISO-style display currency for prices.
        /// </summary>
        public string PriceCurrency { get; set; } = "USD";

        /// <summary>
        /// Price basis such as per person or per group.
        /// </summary>
        public string PriceBasis { get; set; } = string.Empty;

        /// <summary>
        /// Typical activity duration in minutes.
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Supported group size.
        /// </summary>
        public string GroupSize { get; set; } = string.Empty;

        /// <summary>
        /// Cancellation policy summary.
        /// </summary>
        public string Cancellation { get; set; } = string.Empty;

        /// <summary>
        /// Documents required before booking or attending.
        /// </summary>
        public string RequiredDocuments { get; set; } = string.Empty;

        /// <summary>
        /// Whether the activity is currently active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Activity image URLs ordered for display.
        /// </summary>
        public List<string> Images { get; set; } = new();

        /// <summary>
        /// Available external booking providers.
        /// </summary>
        public List<ActivityProviderDto> Providers { get; set; } = new();

        /// <summary>
        /// Opening hours associated with the activity.
        /// </summary>
        public List<ActivityHourDto> OpeningHours { get; set; } = new();
    }

    /// <summary>
    /// External booking provider link for an activity.
    /// </summary>
    public class ActivityProviderDto
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
}
