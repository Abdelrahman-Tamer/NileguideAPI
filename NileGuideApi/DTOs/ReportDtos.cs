namespace NileGuideApi.DTOs
{
    public class UserGrowthDto
    {
        public string Month { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ActivitiesByCategoryDto
    {
        public string Category { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class PopularActivityDto
    {
        public int ActivityId { get; set; }
        public string ActivityName { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    public class ActivityViewsDto
    {
        public string Day { get; set; } = string.Empty;
        public int Views { get; set; }
    }
}