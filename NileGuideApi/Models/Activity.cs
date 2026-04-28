namespace NileGuideApi.Models
{
    // Core domain entity representing a discoverable or bookable activity.
    public class Activity
    {
        public int ActivityID { get; set; }
        public string ActivityName { get; set; } = string.Empty;
        public string? Description { get; set; }

        public int CategoryID { get; set; }
        public Category? Category { get; set; }

        public int CityID { get; set; }
        public City? City { get; set; }

        public decimal? Price { get; set; }
        public decimal? MinPrice { get; set; }
        public string PriceCurrency { get; set; } = "USD";
        public string? PriceBasis { get; set; }
        public int Duration { get; set; }
        public string? GroupSize { get; set; }
        public string? Cancellation { get; set; }
        public string? RequiredDocuments { get; set; }
        public string? Region { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public string? ExternalId { get; set; }
        public string? Provider { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }

        public ICollection<ActivityHour> ActivityHours { get; set; } = new List<ActivityHour>();
        public ICollection<ActivityImage> ActivityImages { get; set; } = new List<ActivityImage>();
        public ICollection<BookingLink> BookingLinks { get; set; } = new List<BookingLink>();
        public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
        public ICollection<PlanItem> PlanItems { get; set; } = new List<PlanItem>();
    }
}
