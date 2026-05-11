namespace NileGuideApi.DTOs
{
    /// <summary>
    /// Opening and closing time for an activity.
    /// </summary>
    public class ActivityHourDto
    {
        /// <summary>
        /// Day label for the opening window.
        /// </summary>
        public string Day { get; set; } = string.Empty;

        public byte OpenHour { get; set; }

        public string OpenAmPm { get; set; } = string.Empty;

        public byte CloseHour { get; set; }

        public string CloseAmPm { get; set; } = string.Empty;

        /// <summary>
        /// Opening time label.
        /// </summary>
        public string OpenTime { get; set; } = string.Empty;

        /// <summary>
        /// Closing time label.
        /// </summary>
        public string CloseTime { get; set; } = string.Empty;
    }
}
