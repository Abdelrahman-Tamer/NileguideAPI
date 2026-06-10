namespace NileGuideApi.Models
{
    public class UserProfile
    {
        public int UserProfileId { get; set; }

        public int UserId { get; set; }

        public User User { get; set; } = null!;

        public bool HasTravelDates { get; set; } = false;

        public DateOnly TravelStartDate { get; set; } = DateOnly.MinValue;

        public DateOnly TravelEndDate { get; set; } = DateOnly.MinValue;

        public string BudgetLevel { get; set; } = string.Empty;

        // Stored as JSON, example: [1,2,3]
        public string PreferredCityIdsJson { get; set; } = "[]";

        // Stored as JSON, example: [1,4,8]
        public string InterestCategoryIdsJson { get; set; } = "[]";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}