namespace NileGuideApi.DTOs
{
    /// <summary>
    /// Generic paged response wrapper.
    /// </summary>
    /// <typeparam name="T">The item type returned in the page.</typeparam>
    public class PagedResultDto<T>
    {
        /// <summary>
        /// Total number of records matching the query.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Current page number.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Number of items requested per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Items returned for the current page.
        /// </summary>
        public List<T> Items { get; set; } = new();
    }
}
