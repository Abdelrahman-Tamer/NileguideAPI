namespace NileGuideApi.DTOs
{
    /// <summary>
    /// Activity category lookup item.
    /// </summary>
    public class CategoryDto
    {
        /// <summary>
        /// Category identifier.
        /// </summary>
        public int CategoryID { get; set; }

        /// <summary>
        /// Category display name.
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;
    }
}
