namespace NileGuideApi.DTOs
{
    public class ActivityDetailsDto
    {
        public int ActivityID { get; set; }

        public string ActivityName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public int CityID { get; set; }
        public string CityName { get; set; } = string.Empty;

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public decimal Price { get; set; }
        public decimal MinPrice { get; set; }

        public string PriceCurrency { get; set; } = "USD";
        public string PriceBasis { get; set; } = string.Empty;

        public string Duration { get; set; } = string.Empty;
        public string GroupSize { get; set; } = string.Empty;
        public string Cancellation { get; set; } = string.Empty;
        public string RequiredDocuments { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public List<string> Images { get; set; } = new();

        public List<ActivityProviderDto> Providers { get; set; } = new();

        public List<ActivityHourDto> OpeningHours { get; set; } = new();
    }

    public class ActivityProviderDto
    {
        public string ProviderName { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
    }
}