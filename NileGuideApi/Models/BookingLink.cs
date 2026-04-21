namespace NileGuideApi.Models
{
    // External provider link used to book a specific activity.
    public class BookingLink
    {
        public int Id { get; set; }

        public int ActivityID { get; set; }
        public Activity? Activity { get; set; }

        public string Provider { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
