using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NileGuideApi.DTOs;
using NileGuideApi.Services;

namespace NileGuideApi.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Route("api/admin/activities")]
    [ApiController]
    [Produces("application/json")]
    public class AdminActivitiesController : ControllerBase
    {
        private readonly IAdminActivityService _adminActivityService;

        public AdminActivitiesController(IAdminActivityService adminActivityService)
        {
            _adminActivityService = adminActivityService;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateActivityDto dto)
        {
            try
            {
                var result = await _adminActivityService.CreateAsync(dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromForm] UpdateActivityDto dto)
        {
            try
            {
                var result = await _adminActivityService.UpdateAsync(id, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                await _adminActivityService.DeleteAsync(id);
                return Ok(new { message = "Activity deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}