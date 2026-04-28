using System.ComponentModel.DataAnnotations;

namespace NileGuideApi.DTOs
{
    /// <summary>
    /// Query-string filters used by the wishlist listing endpoint.
    /// </summary>
    public class WishlistFilterDto
    {
        /// <summary>
        /// Page number starting from 1.
        /// </summary>
        [Range(1, 10000, ErrorMessage = "Page must be between 1 and 10000")]
        public int Page { get; set; } = 1;

        /// <summary>
        /// Number of wishlist activities per page.
        /// </summary>
        [Range(1, 50, ErrorMessage = "PageSize must be between 1 and 50")]
        public int PageSize { get; set; } = 9;
    }

    /// <summary>
    /// Wishlist membership status for an activity.
    /// </summary>
    public class WishlistStatusDto
    {
        /// <summary>
        /// Activity identifier.
        /// </summary>
        public int ActivityID { get; set; }

        /// <summary>
        /// Whether the current user has saved this activity.
        /// </summary>
        public bool IsWishlisted { get; set; }
    }
}
