using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NileGuideApi.Data;
using NileGuideApi.DTOs;
using NileGuideApi.Services;
using System.Security.Claims;

namespace NileGuideApi.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private const long MaxImageBytes = 5 * 1024 * 1024;

        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        private readonly AppDbContext _context;
        private readonly IProfilePictureService _profilePictureService;

        public UsersController(AppDbContext context, IProfilePictureService profilePictureService)
        {
            _context = context;
            _profilePictureService = profilePictureService;
        }

        [HttpPost("me/profile-picture")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(MaxImageBytes)]
        [ProducesResponseType(typeof(ProfilePictureResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status502BadGateway)]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> UploadMyProfilePicture([FromForm] ProfilePictureUploadRequest request)
        {
            var authenticatedUserId = GetAuthenticatedUserId();
            if (authenticatedUserId == null)
                return Unauthorized(new { message = "Invalid token" });

            var validationError = ValidateImage(request.Image);
            if (validationError != null)
                return BadRequest(new { message = validationError });

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == authenticatedUserId.Value);
            if (user == null)
                return NotFound(new { message = "User not found" });

            try
            {
                var oldProfilePictureUrl = user.ProfilePictureUrl;
                var imageUrl = await _profilePictureService.UploadAsync(request.Image!, user.Id);
                user.ProfilePictureUrl = imageUrl;
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                await _profilePictureService.DeleteByUrlAsync(oldProfilePictureUrl, user.Id);

                return Ok(new ProfilePictureResponseDto { ProfilePictureUrl = imageUrl });
            }
            catch (InvalidOperationException ex) when (ex.Message == "Cloudinary is not configured")
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Cloudinary is not configured" });
            }
            catch (InvalidOperationException)
            {
                return StatusCode(StatusCodes.Status502BadGateway, new { message = "Profile picture upload failed" });
            }
        }

        private static string? ValidateImage(IFormFile? image)
        {
            if (image == null || image.Length == 0)
                return "Image is required";

            if (image.Length > MaxImageBytes)
                return "Image must be at most 5MB";

            if (!AllowedContentTypes.Contains(image.ContentType))
                return "Image must be jpg, png, or webp";

            var extension = Path.GetExtension(image.FileName);
            if (!AllowedExtensions.Contains(extension))
                return "Image must be jpg, png, or webp";

            return null;
        }

        private int? GetAuthenticatedUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idStr, out var userId) ? userId : null;
        }
    }
}
