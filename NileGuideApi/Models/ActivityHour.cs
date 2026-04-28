namespace NileGuideApi.Models
{
    // Daily opening/closing window stored separately from the core activity row.
    public class ActivityHour
    {
        public int Id { get; set; }

        public int ActivityID { get; set; }
        public Activity? Activity { get; set; }

        public byte OpenHour { get; set; }
        public string OpenAmPm { get; set; } = "AM";
        public byte CloseHour { get; set; }
        public string CloseAmPm { get; set; } = "PM";
    }
}
