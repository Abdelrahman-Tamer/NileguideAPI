using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NileGuideApi.Data;
using NileGuideApi.DTOs;
using NileGuideApi.Models;
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

        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] string? role,
            [FromQuery] bool? isActive,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 100);

            var query = _context.Users
                .AsNoTracking()
                .Include(x => x.WishlistItems)
                .Where(x => x.DeletedAt == null)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalizedSearch = search.Trim().ToLower();

                query = query.Where(x =>
                    x.FullName.ToLower().Contains(normalizedSearch) ||
                    x.Email.ToLower().Contains(normalizedSearch));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                var normalizedRole = role.Trim();

                if (string.Equals(normalizedRole, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x => x.Role == "Admin");
                }
                else if (string.Equals(normalizedRole, "User", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x => x.Role == "Tourist" || x.Role == "User");
                }
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new UserListItemDto
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    Email = x.Email,
                    Role = MapDbRoleToUiRole(x.Role),
                    Joined = x.CreatedAt,
                    WishlistItems = x.WishlistItems.Count,
                    IsActive = x.IsActive,
                    ProfilePictureUrl = x.ProfilePictureUrl
                })
                .ToListAsync();

            return Ok(new
            {
                totalCount,
                page,
                pageSize,
                items
            });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Include(x => x.WishlistItems)
                .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);

            if (user == null)
                return NotFound(new { message = "User not found" });

            var dto = new UserDetailsDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = MapDbRoleToUiRole(user.Role),
                Joined = user.CreatedAt,
                LastSeen = null,
                WishlistItems = user.WishlistItems.Count,
                IsActive = user.IsActive,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Nationality = user.Nationality,
                DateOfBirth = user.DateOfBirth
            };

            return Ok(dto);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            var email = (dto.Email ?? string.Empty).Trim().ToLowerInvariant();

            var exists = await _context.Users.AnyAsync(x => x.Email == email && x.DeletedAt == null);
            if (exists)
                return Conflict(new { message = "Email already exists" });

            var user = new User
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName.Trim(),
                Nationality = dto.Nationality.Trim(),
                DateOfBirth = dto.DateOfBirth,
                ProfilePictureUrl = string.IsNullOrWhiteSpace(dto.ProfilePictureUrl)
                    ? null
                    : dto.ProfilePictureUrl.Trim(),
                Role = MapUiRoleToDbRole(dto.Role),
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = new UserDetailsDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = MapDbRoleToUiRole(user.Role),
                Joined = user.CreatedAt,
                LastSeen = null,
                WishlistItems = 0,
                IsActive = user.IsActive,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Nationality = user.Nationality,
                DateOfBirth = user.DateOfBirth
            };

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, result);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPatch("{id:int}/role")]
        public async Task<IActionResult> UpdateRole([FromRoute] int id, [FromBody] UpdateUserRoleDto dto)
        {
            var user = await _context.Users
                .Include(x => x.WishlistItems)
                .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);

            if (user == null)
                return NotFound(new { message = "User not found" });

            user.Role = MapUiRoleToDbRole(dto.Role);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new UserDetailsDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = MapDbRoleToUiRole(user.Role),
                Joined = user.CreatedAt,
                LastSeen = null,
                WishlistItems = user.WishlistItems.Count,
                IsActive = user.IsActive,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Nationality = user.Nationality,
                DateOfBirth = user.DateOfBirth
            });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus([FromRoute] int id, [FromBody] UpdateUserStatusDto dto)
        {
            var user = await _context.Users
                .Include(x => x.WishlistItems)
                .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);

            if (user == null)
                return NotFound(new { message = "User not found" });

            user.IsActive = dto.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new UserDetailsDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = MapDbRoleToUiRole(user.Role),
                Joined = user.CreatedAt,
                LastSeen = null,
                WishlistItems = user.WishlistItems.Count,
                IsActive = user.IsActive,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Nationality = user.Nationality,
                DateOfBirth = user.DateOfBirth
            });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);

            if (user == null)
                return NotFound(new { message = "User not found" });

            user.DeletedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "User deleted successfully" });
        }

        [HttpPost("me/profile-picture")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(MaxImageBytes)]
        public async Task<IActionResult> UploadMyProfilePicture([FromForm] ProfilePictureUploadRequest request)
        {
            var authenticatedUserId = GetAuthenticatedUserId();
            if (authenticatedUserId == null)
                return Unauthorized(new { message = "Invalid token" });

            var validationError = ValidateImage(request.Image);
            if (validationError != null)
                return BadRequest(new { message = validationError });

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == authenticatedUserId.Value && x.DeletedAt == null);
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

        private static string MapDbRoleToUiRole(string role)
        {
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
                return "Admin";

            return "User";
        }

        private static string MapUiRoleToDbRole(string role)
        {
            if (string.Equals(role?.Trim(), "Admin", StringComparison.OrdinalIgnoreCase))
                return "Admin";

            return "Tourist";
        }
    }
}