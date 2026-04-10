namespace NileGuideApi.Models
{
    // Core domain entity representing a discoverable or bookable activity.
    public class Activity
    {
        public int ActivityID { get; set; }
        public string ActivityName { get; set; } = string.Empty;

        public int CategoryID { get; set; }
        public Category? Category { get; set; }

        public int CityID { get; set; }
        public City? City { get; set; }

        public string? Description { get; set; }
        public decimal? PriceMinEst { get; set; }
        public decimal? PriceMaxEst { get; set; }
        public string PriceCurrency { get; set; } = "USD";
        public string? PriceBasis { get; set; }
        public string? OpeningHours { get; set; }
        public string? Location { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public bool RequiresPersonalID { get; set; }
        public string Status { get; set; } = "Available";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }

        public ICollection<ActivityImage> ActivityImages { get; set; } = new List<ActivityImage>();
    }
}
