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
        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<ActivityCardDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetActivities([FromQuery] ActivityFilterDto filter)
        {
            var result = await _activityService.GetActivitiesAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Gets multiple activities by IDs in a single batch request.
        /// </summary>
        [HttpPost("batch")]
        [ProducesResponseType(typeof(List<ActivityCardDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActivitiesBatch([FromBody] List<int> ids)
        {
            if (ids == null || ids.Count == 0)
                return Ok(new List<ActivityCardDto>());

            var activities = await _activityService.GetActivitiesBatchAsync(ids);
            return Ok(activities);
        }

        /// <summary>
        /// Records an activity view explicitly without side effects on GET requests.
        /// </summary>
        [HttpPost("{id}/view")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RecordActivityView(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Activity id must be positive" });

            var success = await _activityService.RecordViewAsync(id);
            if (!success)
                return NotFound(new { message = "Activity not found" });

            return Ok(new { message = "View recorded successfully" });
        }

        /// <summary>
        /// Gets a single activity by id.
        /// </summary>
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
