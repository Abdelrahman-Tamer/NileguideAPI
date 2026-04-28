using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NileGuideApi.DTOs;
using NileGuideApi.Services;
using System.Security.Claims;

namespace NileGuideApi.Controllers
{
    /// <summary>
    /// Authenticated user's saved activity wishlist.
    /// </summary>
    [Authorize]
    [Route("api/wishlist")]
    [ApiController]
    [Produces("application/json")]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        /// <summary>
        /// Gets the current user's saved activities.
        /// </summary>
        /// <param name="filter">Pagination query values.</param>
        /// <returns>A paged list of saved activities ordered by newest save first.</returns>
        /// <response code="200">Returns the current user's wishlist page.</response>
        /// <response code="400">Returned when pagination values are invalid.</response>
        /// <response code="401">Returned when the bearer token is missing or invalid.</response>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<ActivityCardDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetWishlist([FromQuery] WishlistFilterDto filter)
        {
            var userId = GetAuthenticatedUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token" });

            var result = await _wishlistService.GetWishlistAsync(userId.Value, filter);
            return Ok(result);
        }

        /// <summary>
        /// Gets all activity ids saved by the current user.
        /// </summary>
        /// <remarks>
        /// Use this endpoint to render wishlist heart states across activity cards with one request.
        /// </remarks>
        /// <returns>The saved activity ids ordered by newest save first.</returns>
        /// <response code="200">Returns activity ids saved by the current user.</response>
        /// <response code="401">Returned when the bearer token is missing or invalid.</response>
        [HttpGet("activity-ids")]
        [ProducesResponseType(typeof(List<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetActivityIds()
        {
            var userId = GetAuthenticatedUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token" });

            var activityIds = await _wishlistService.GetActivityIdsAsync(userId.Value);
            return Ok(activityIds);
        }

        /// <summary>
        /// Adds an activity to the current user's wishlist.
        /// </summary>
        /// <param name="activityId">Positive activity identifier.</param>
        /// <returns>A message describing whether the activity was added or already saved.</returns>
        /// <response code="200">The activity was added or was already in the wishlist.</response>
        /// <response code="400">Returned when the activity id is not positive.</response>
        /// <response code="401">Returned when the bearer token is missing or invalid.</response>
        /// <response code="404">Returned when the activity does not exist.</response>
        [HttpPost("{activityId}")]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Add(int activityId)
        {
            if (activityId <= 0)
                return BadRequest(new { message = "Activity id must be positive" });

            var userId = GetAuthenticatedUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token" });

            var result = await _wishlistService.AddAsync(userId.Value, activityId);

            return result switch
            {
                WishlistAddResult.Added => Ok(new { message = "Activity added to wishlist" }),
                WishlistAddResult.AlreadyExists => Ok(new { message = "Activity already in wishlist" }),
                WishlistAddResult.ActivityNotFound => NotFound(new { message = "Activity not found" }),
                _ => throw new InvalidOperationException("Unexpected wishlist add result")
            };
        }

        /// <summary>
        /// Removes an activity from the current user's wishlist.
        /// </summary>
        /// <param name="activityId">Positive activity identifier.</param>
        /// <returns>A removal confirmation message.</returns>
        /// <response code="200">The wishlist no longer contains the activity.</response>
        /// <response code="400">Returned when the activity id is not positive.</response>
        /// <response code="401">Returned when the bearer token is missing or invalid.</response>
        [HttpDelete("{activityId}")]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Remove(int activityId)
        {
            if (activityId <= 0)
                return BadRequest(new { message = "Activity id must be positive" });

            var userId = GetAuthenticatedUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token" });

            await _wishlistService.RemoveAsync(userId.Value, activityId);
            return Ok(new { message = "Activity removed from wishlist" });
        }

        /// <summary>
        /// Checks whether an activity is saved in the current user's wishlist.
        /// </summary>
        /// <param name="activityId">Positive activity identifier.</param>
        /// <returns>Wishlist membership status for the activity.</returns>
        /// <response code="200">Returns whether the activity is saved.</response>
        /// <response code="400">Returned when the activity id is not positive.</response>
        /// <response code="401">Returned when the bearer token is missing or invalid.</response>
        /// <response code="404">Returned when the activity does not exist.</response>
        [HttpGet("status/{activityId}")]
        [ProducesResponseType(typeof(WishlistStatusDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStatus(int activityId)
        {
            if (activityId <= 0)
                return BadRequest(new { message = "Activity id must be positive" });

            var userId = GetAuthenticatedUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token" });

            var status = await _wishlistService.GetStatusAsync(userId.Value, activityId);
            if (status == null)
                return NotFound(new { message = "Activity not found" });

            return Ok(status);
        }

        private int? GetAuthenticatedUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idStr, out var userId) ? userId : null;
        }
    }
}
