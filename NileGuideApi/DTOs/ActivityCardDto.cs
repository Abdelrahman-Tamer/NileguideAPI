namespace NileGuideApi.DTOs
{
    public class ActivityCardDto
    {
        public int ActivityID { get; set; }

        public string ActivityName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public int CityID { get; set; }
        public string CityName { get; set; } = string.Empty;

        public decimal MinPrice { get; set; }
        public string PriceCurrency { get; set; } = "USD";

        public string ImageUrl { get; set; } = string.Empty;
        public string RequiredDocuments { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public double Rating { get; set; } = 5.0;
        public int ReviewsCount { get; set; } = 89;

        public List<ActivityProviderDto> Providers { get; set; } = new();
        public List<ActivityHourDto> OpeningHours { get; set; } = new();
    }
}