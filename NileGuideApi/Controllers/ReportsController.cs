using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NileGuideApi.Services;

namespace NileGuideApi.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Route("api/reports")]
    [ApiController]
    [Produces("application/json")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("activity-views")]
        public async Task<IActionResult> GetActivityViews()
        {
            var result = await _reportService.GetActivityViewsLast7DaysAsync();
            return Ok(result);
        }

        [HttpGet("user-growth")]
        public async Task<IActionResult> GetUserGrowth()
        {
            var result = await _reportService.GetUserGrowthAsync();
            return Ok(result);
        }

        [HttpGet("activities-by-category")]
        public async Task<IActionResult> GetActivitiesByCategory()
        {
            var result = await _reportService.GetActivitiesByCategoryAsync();
            return Ok(result);
        }

        [HttpGet("top-activities")]
        public async Task<IActionResult> GetTopActivities([FromQuery] int top = 5)
        {
            var result = await _reportService.GetTopPopularActivitiesAsync(top);
            return Ok(result);
        }
    }
}