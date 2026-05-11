namespace NileGuideApi.DTOs
{
    /// <summary>
    /// Supported city lookup item.
    /// </summary>
    public class CityDto
    {
        /// <summary>
        /// City identifier.
        /// </summary>
        public int CityID { get; set; }

        /// <summary>
        /// City display name.
        /// </summary>
        public string CityName { get; set; } = string.Empty;
    }
}
