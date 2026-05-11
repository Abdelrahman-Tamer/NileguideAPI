using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NileGuideApi.DTOs;
using NileGuideApi.Services;
using System.Security.Claims;

namespace NileGuideApi.Controllers
{
    /// <summary>
    /// Authenticated user's scheduled activity plan.
    /// </summary>
    [Authorize]
    [Route("api/plan")]
    [ApiController]
    [Produces("application/json")]
    public class PlanController : ControllerBase
    {
        private readonly IPlanService _planService;

        public PlanController(IPlanService planService)
        {
            _planService = planService;
        }

        /// <summary>
        /// Gets the current user's activity plan.
        /// </summary>
        /// <returns>The current user's scheduled activity plan.</returns>
        /// <response code="200">Returns the user's plan.</response>
        /// <response code="401">Returned when the bearer token is missing or invalid.</response>
        [HttpGet]
        [ProducesResponseType(typeof(PlanResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPlan()
        {
            var userId = GetAuthenticatedUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token" });

            var result = await _planService.GetPlanAsync(userId.Value);
            return Ok(result);
        }

        /// <summary>
        /// Adds an activity to the current user's plan.
        /// </summary>
        /// <param name="request">Activity and schedule values.</param>
        /// <returns>The saved plan item.</returns>
        /// <response code="200">Returns the newly added or already existing plan item.</response>
        /// <response code="400">Returned when activity id or time values are invalid.</response>
        /// <response code="401">Returned when the bearer token is missing or invalid.</response>
        /// <response code="404">Returned when the activity does not exist.</response>
        [HttpPost("items")]
        [ProducesResponseType(typeof(PlanItemResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddItem(AddPlanItemRequest request)
        {
            var userId = GetAuthenticatedUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token" });

            try
            {
                var result = await _planService.AddItemAsync(userId.Value, request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Removes an activity row from the current user's plan.
        /// </summary>
        /// <param name="planItemId">Plan item identifier.</param>
        /// <returns>No content when the plan item is removed.</returns>
        /// <response code="204">The plan item was removed.</response>
        /// <response code="400">Returned when the plan item id is not positive.</response>
        /// <response code="401">Returned when the bearer token is missing or invalid.</response>
        /// <response code="404">Returned when the plan item does not exist.</response>
        [HttpDelete("items/{planItemId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteItem(int planItemId)
        {
            if (planItemId <= 0)
                return BadRequest(new { message = "Plan item id must be positive" });

            var userId = GetAuthenticatedUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token" });

            try
            {
                await _planService.DeleteItemAsync(userId.Value, planItemId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Gets all activity ids currently in the user's plan.
        /// </summary>
        /// <returns>Activity ids ordered by scheduled date and time.</returns>
        /// <response code="200">Returns planned activity ids.</response>
        /// <response code="401">Returned when the bearer token is missing or invalid.</response>
        [HttpGet("activity-ids")]
        [ProducesResponseType(typeof(List<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetActivityIds()
        {
            var userId = GetAuthenticatedUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token" });

            var activityIds = await _planService.GetActivityIdsAsync(userId.Value);
            return Ok(activityIds);
        }

        private int? GetAuthenticatedUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idStr, out var userId) ? userId : null;
        }
    }
}
