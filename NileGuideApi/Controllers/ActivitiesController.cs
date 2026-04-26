using Microsoft.AspNetCore.Mvc;
using NileGuideApi.DTOs;
using NileGuideApi.Services;

namespace NileGuideApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivitiesController : ControllerBase
    {
        private readonly IActivityService _activityService;

        public ActivitiesController(IActivityService activityService)
        {
            _activityService = activityService;
        }

        [HttpGet]
        public async Task<IActionResult> GetActivities([FromQuery] ActivityFilterDto filter)
        {
            var result = await _activityService.GetActivitiesAsync(filter);
            return Ok(result);
        }

        [HttpGet("{id}")]
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
