namespace NileGuideApi.DTOs
{
    /// <summary>
    /// Contains the main values displayed in the admin dashboard cards.
    /// </summary>
    public class DashboardDto
    {
        public int TotalUsers { get; set; }
        public int TotalActivities { get; set; }
        public int TotalReviews { get; set; }
        public int WishlistItems { get; set; }
        public double AverageRating { get; set; }
    }
}