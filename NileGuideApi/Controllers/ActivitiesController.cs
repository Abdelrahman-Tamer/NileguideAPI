using Microsoft.AspNetCore.Mvc;
using NileGuideApi.DTOs;
using NileGuideApi.Services;

namespace NileGuideApi.Controllers
{
    /// <summary>
    /// Public activity discovery endpoints used by the frontend catalog.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ActivitiesController : ControllerBase
    {
        private readonly IActivityService _activityService;

        public ActivitiesController(IActivityService activityService)
        {
            _activityService = activityService;
        }

        /// <summary>
        /// Gets a paged list of active activities.
        /// </summary>
        /// <remarks>
        /// Supports optional category, city, search, sorting, and pagination filters.
        /// </remarks>
        /// <param name="filter">Query-string filters used to narrow and page the activity list.</param>
        /// <returns>A paged activity list.</returns>
        /// <response code="200">Returns the filtered page of activities.</response>
        /// <response code="400">Returned when filters or pagination values are invalid.</response>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<ActivityCardDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetActivities([FromQuery] ActivityFilterDto filter)
        {
            var result = await _activityService.GetActivitiesAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Gets a single activity by id.
        /// </summary>
        /// <param name="id">Positive activity identifier.</param>
        /// <returns>The requested activity details.</returns>
        /// <response code="200">Returns the activity details.</response>
        /// <response code="400">Returned when the activity id is not positive.</response>
        /// <response code="404">Returned when the activity does not exist.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ActivityDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetActivityById(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Activity id must be positive" });

            var activity = await _activityService.GetActivityByIdAsync(id);

            if (activity == null)
                return NotFound(new { message = "Activity not found" });

            return Ok(activity);
        }
    }
}
