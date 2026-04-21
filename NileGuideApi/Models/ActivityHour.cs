namespace NileGuideApi.Models
{
    // Daily opening/closing window stored separately from the core activity row.
    public class ActivityHour
    {
        public int Id { get; set; }

        public int ActivityID { get; set; }
        public Activity? Activity { get; set; }

        public byte? OpeningHour { get; set; }
        public string? OpeningPeriod { get; set; }
        public byte? ClosingHour { get; set; }
        public string? ClosingPeriod { get; set; }
    }
}
