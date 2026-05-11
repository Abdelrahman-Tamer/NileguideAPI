namespace NileGuideApi.Models
{
    // Lookup entity used to store supported cities and their regional metadata.
    public class City
    {
        public int CityID { get; set; }
        public string CityName { get; set; } = string.Empty;
        public string? Region { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool IsPopular { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }

        public ICollection<Activity> Activities { get; set; } = new List<Activity>();
    }
}
