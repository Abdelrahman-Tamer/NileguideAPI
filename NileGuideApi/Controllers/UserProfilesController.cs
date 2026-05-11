using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NileGuideApi.DTOs;
using NileGuideApi.Services;

namespace NileGuideApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/users/me/profile")]
    public class UserProfilesController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;

        public UserProfilesController(IUserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetAuthenticatedUserId();

            if (userId == null)
                return Unauthorized(new { message = "Invalid token" });

            var profile = await _userProfileService.GetMyProfileAsync(userId.Value);

            if (profile == null)
                return NotFound(new { message = "User not found" });

            return Ok(profile);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateUserProfileDto dto)
        {
            var userId = GetAuthenticatedUserId();

            if (userId == null)
                return Unauthorized(new { message = "Invalid token" });

            try
            {
                var profile = await _userProfileService.UpdateMyProfileAsync(userId.Value, dto);

                if (profile == null)
                    return NotFound(new { message = "User not found" });

                return Ok(profile);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private int? GetAuthenticatedUserId()
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("nameid") ??
                User.FindFirstValue("sub") ??
                User.FindFirstValue("id") ??
                User.FindFirstValue("userId");

            if (int.TryParse(userIdValue, out var userId))
                return userId;

            return null;
        }
    }
}