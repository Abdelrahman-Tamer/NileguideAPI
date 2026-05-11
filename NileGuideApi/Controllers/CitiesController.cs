using Microsoft.AspNetCore.Mvc;
using NileGuideApi.DTOs;
using NileGuideApi.Services;

namespace NileGuideApi.Controllers
{
    /// <summary>
    /// Public lookup endpoints for supported cities.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class CitiesController : ControllerBase
    {
        private readonly ICityService _cityService;

        public CitiesController(ICityService cityService)
        {
            _cityService = cityService;
        }

        /// <summary>
        /// Gets all supported cities.
        /// </summary>
        /// <returns>The supported city list ordered for display.</returns>
        /// <response code="200">Returns all supported cities.</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<CityDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCities()
        {
            var cities = await _cityService.GetAllCitiesAsync();
            return Ok(cities);
        }
    }
}
