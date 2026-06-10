using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NileGuideApi.DTOs;
using NileGuideApi.Services;

namespace NileGuideApi.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Route("api/dashboard")]
    [ApiController]
    [Produces("application/json")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }
        /// <summary>
        /// Returns the values used in the admin dashboard cards.
        /// </summary>
        /// <returns>Dashboard values from the current database state.</returns>

        [HttpGet]
        [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get()
        {
            var result = await _dashboardService.GetAsync();
            return Ok(result);
        }
    }
}