using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NileGuideApi.Data;
using NileGuideApi.DTOs;
using NileGuideApi.Models;
using NileGuideApi.Services;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace NileGuideApi.Controllers
{
    // Handles account creation, login, profile lookup, and password reset flows.
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IAuthTokenService _authTokenService;
        private readonly IEmailSender _emailSender;
        private readonly IEmailTemplateService _emailTemplateService;

        public AuthController(
            AppDbContext context,
            IConfiguration config,
            IAuthTokenService authTokenService,
            IEmailSender emailSender,
            IEmailTemplateService emailTemplateService)
        {
            _context = context;
            _config = config;
            _authTokenService = authTokenService;
            _emailSender = emailSender;
            _emailTemplateService = emailTemplateService;
        }

        // Registers a new tourist user and immediately returns a JWT for the frontend session.
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var email = (dto.Email ?? "").Trim().ToLowerInvariant();

            // Emails are normalized before saving so duplicate checks stay consistent.
            var exists = await _context.Users.AnyAsync(u => u.Email == email);
            if (exists)
                return Conflict(new { message = "Email already exists" });

            var user = new User
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName.Trim(),
                Nationality = dto.Nationality.Trim(),
                Role = "Tourist",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                // The unique index is the final guard against concurrent duplicate registration.
                return Conflict(new { message = "Email already exists" });
            }

            var authResponse = await CreateAuthResponseAsync(user, rememberMe: false);
            return Ok(authResponse);
        }

        // Verifies credentials and returns the existing JWT fields plus refresh-token data.
        [EnableRateLimiting("LoginPolicy")]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var email = (dto.Email ?? "").Trim().ToLowerInvariant();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return Unauthorized(new { message = "Invalid credentials" });

            if (!user.IsActive || user.DeletedAt != null)
                return Unauthorized(new { message = "Invalid credentials" });

            var ok = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!ok)
                return Unauthorized(new { message = "Invalid credentials" });

            var authResponse = await CreateAuthResponseAsync(user, dto.RememberMe);
            return Ok(authResponse);
        }

        // Returns the profile represented by the current JWT.
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idStr, out var id))
                return Unauthorized(new { message = "Invalid token" });

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
                return Unauthorized(new { message = "User not found" });

            // A valid token should still be rejected if the account was later disabled or deleted.
            if (!user.IsActive || user.DeletedAt != null)
                return Unauthorized(new { message = "Invalid token" });

            return Ok(new
            {
                userId = user.Id,
                email = user.Email,
                fullName = user.FullName,
                nationality = user.Nationality,
                role = user.Role
            });
        }

        // Rotates a valid refresh token and issues a brand-new token pair.
        [EnableRateLimiting("LoginPolicy")]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
        {
            RefreshToken? refreshToken;

            try
            {
                var tokenHash = _authTokenService.HashRefreshToken(dto.RefreshToken);

                refreshToken = await _context.RefreshTokens
                    .IgnoreQueryFilters()
                    .Include(x => x.User)
                    .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);
            }
            catch (InvalidOperationException)
            {
                return Unauthorized(new { message = "Invalid refresh token" });
            }

            if (refreshToken?.User == null)
                return Unauthorized(new { message = "Invalid refresh token" });

            var now = DateTime.UtcNow;
            if (refreshToken.RevokedAt != null)
            {
                await RevokeDescendantRefreshTokensOnReuseAsync(refreshToken, GetClientIp());
                return Unauthorized(new { message = "Invalid refresh token" });
            }

            if (refreshToken.ExpiresAt <= now)
                return Unauthorized(new { message = "Invalid refresh token" });

            if (!refreshToken.User.IsActive || refreshToken.User.DeletedAt != null)
                return Unauthorized(new { message = "Invalid refresh token" });

            var rememberMe = _authTokenService.UsesRememberMeLifetime(refreshToken);
            var (newAccessToken, accessTokenExpiresAtUtc) = _authTokenService.GenerateAccessToken(refreshToken.User);
            var (newRefreshToken, plainTextRefreshToken) = _authTokenService.GenerateRefreshToken(
                refreshToken.User,
                rememberMe,
                GetClientIp());

            RevokeRefreshToken(refreshToken, GetClientIp(), newRefreshToken.TokenHash);

            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();

            return Ok(BuildAuthResponse(
                refreshToken.User,
                newAccessToken,
                accessTokenExpiresAtUtc,
                plainTextRefreshToken,
                newRefreshToken.ExpiresAt));
        }

        // Revokes all active refresh tokens for the current authenticated user.
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = GetAuthenticatedUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token" });

            var now = DateTime.UtcNow;
            var ipAddress = GetClientIp();

            var activeRefreshTokens = await _context.RefreshTokens
                .IgnoreQueryFilters()
                .Where(x => x.UserId == userId.Value && x.RevokedAt == null && x.ExpiresAt > now)
                .ToListAsync();

            foreach (var refreshToken in activeRefreshTokens)
                RevokeRefreshToken(refreshToken, ipAddress);

            if (activeRefreshTokens.Count > 0)
                await _context.SaveChangesAsync();

            return Ok(new { message = "Logged out" });
        }

        // Creates a short-lived reset token and emails a 6-digit code to the user.
        [EnableRateLimiting("ResetPolicy")]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var email = (dto.Email ?? "").Trim().ToLowerInvariant();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            // Keep the response generic so callers cannot enumerate valid accounts.
            if (user == null || !user.IsActive || user.DeletedAt != null)
                return Ok(new { message = "If the email exists, a reset code was sent." });

            var now = DateTime.UtcNow;
            var code = Generate6DigitCode();
            var tokenHash = HashResetCode(user.Id, code);
            var existingTokenIds = await _context.PasswordResetTokens
                .Where(t => t.UserId == user.Id && t.UsedAt == null && t.ExpiresAt > now)
                .Select(t => t.Id)
                .ToListAsync();

            var token = new PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                CreatedAt = now,
                ExpiresAt = now.AddMinutes(10),
                AttemptCount = 0
            };

            _context.PasswordResetTokens.Add(token);
            await _context.SaveChangesAsync();

            var emailContent = _emailTemplateService.BuildPasswordResetCodeEmail(
                code,
                TimeSpan.FromMinutes(10));

            try
            {
                await _emailSender.SendEmailAsync(
                    user.Email,
                    "NileGuide Password Reset Code",
                    emailContent.PlainTextBody,
                    emailContent.HtmlBody);

                if (existingTokenIds.Count == 0)
                    return Ok(new { message = "If the email exists, a reset code was sent." });

                var previousTokens = await _context.PasswordResetTokens
                    .Where(t => existingTokenIds.Contains(t.Id))
                    .ToListAsync();

                foreach (var t in previousTokens)
                    t.UsedAt = now;
                
                await _context.SaveChangesAsync();
            }
            catch
            {
                token.UsedAt = DateTime.UtcNow;
                token.LastAttemptAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                throw;
            }

            return Ok(new { message = "If the email exists, a reset code was sent." });
        }

        // Verifies the latest valid reset code without consuming it yet.
        [EnableRateLimiting("ResetPolicy")]
        [HttpPost("verify-reset-code")]
        public async Task<IActionResult> VerifyResetCode([FromBody] VerifyResetCodeDto dto)
        {
            var email = (dto.Email ?? "").Trim().ToLowerInvariant();
            var code = (dto.Code ?? "").Trim();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return BadRequest(new { message = "Invalid code" });

            var tokenHash = HashResetCode(user.Id, code);

            var token = await _context.PasswordResetTokens
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync(x =>
                    x.UserId == user.Id &&
                    x.TokenHash == tokenHash &&
                    x.UsedAt == null &&
                    x.ExpiresAt > DateTime.UtcNow &&
                    x.AttemptCount < 5);

            if (token == null)
            {
                await IncrementLatestResetAttempt(user.Id);
                return BadRequest(new { message = "Invalid code" });
            }

            // A successful verification should not count as a failed attempt.
            return Ok(new { message = "Code is valid" });
        }

        // Replaces the user's password after a valid reset-code check.
        [EnableRateLimiting("ResetPolicy")]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var email = (dto.Email ?? "").Trim().ToLowerInvariant();
            var code = (dto.Code ?? "").Trim();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return BadRequest(new { message = "Invalid code" });

            var tokenHash = HashResetCode(user.Id, code);

            var token = await _context.PasswordResetTokens
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync(x =>
                    x.UserId == user.Id &&
                    x.TokenHash == tokenHash &&
                    x.UsedAt == null &&
                    x.ExpiresAt > DateTime.UtcNow &&
                    x.AttemptCount < 5);

            if (token == null)
            {
                await IncrementLatestResetAttempt(user.Id);
                return BadRequest(new { message = "Invalid code" });
            }

            // Prevent the reset flow from being used to keep the same password.
            if (BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.PasswordHash))
                return BadRequest(new { message = "New password cannot be the same as the old password" });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            token.UsedAt = DateTime.UtcNow;
            token.LastAttemptAt = DateTime.UtcNow;

            await RevokeActiveRefreshTokensAsync(user.Id, GetClientIp());
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password updated" });
        }

        // Creates and stores a fresh token pair while preserving the existing login/register response shape.
        private async Task<AuthTokenResponseDto> CreateAuthResponseAsync(User user, bool rememberMe)
        {
            var (accessToken, accessTokenExpiresAtUtc) = _authTokenService.GenerateAccessToken(user);
            var (refreshToken, plainTextRefreshToken) = _authTokenService.GenerateRefreshToken(
                user,
                rememberMe,
                GetClientIp());

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return BuildAuthResponse(
                user,
                accessToken,
                accessTokenExpiresAtUtc,
                plainTextRefreshToken,
                refreshToken.ExpiresAt);
        }

        // Generates the human-entered six-digit code emailed to the user.
        private string Generate6DigitCode()
        {
            var n = RandomNumberGenerator.GetInt32(0, 1_000_000);
            return n.ToString("D6");
        }

        // Hashes reset codes so the raw code is never stored in the database.
        private string HashResetCode(int userId, string code)
        {
            var pepper = _config["Security:ResetCodePepper"];
            if (string.IsNullOrWhiteSpace(pepper))
                throw new InvalidOperationException("Security:ResetCodePepper is missing");

            var raw = $"{userId}:{code}:{pepper}";
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
            return Convert.ToHexString(bytes);
        }

        // Converts SQL Server unique-index violations into the same conflict response the frontend already expects.
        private static bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            return ex.InnerException is SqlException { Number: 2601 or 2627 };
        }

        // Tracks failed attempts against the newest active reset token for a user.
        private async Task IncrementLatestResetAttempt(int userId)
        {
            var latest = await _context.PasswordResetTokens
                .Where(t => t.UserId == userId && t.UsedAt == null && t.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync();

            if (latest == null) return;

            latest.AttemptCount += 1;
            latest.LastAttemptAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        private async Task RevokeActiveRefreshTokensAsync(int userId, string? revokedByIp)
        {
            var now = DateTime.UtcNow;

            var activeRefreshTokens = await _context.RefreshTokens
                .IgnoreQueryFilters()
                .Where(x => x.UserId == userId && x.RevokedAt == null && x.ExpiresAt > now)
                .ToListAsync();

            foreach (var refreshToken in activeRefreshTokens)
                RevokeRefreshToken(refreshToken, revokedByIp);
        }

        private async Task RevokeDescendantRefreshTokensOnReuseAsync(RefreshToken reusedRefreshToken, string? revokedByIp)
        {
            if (string.IsNullOrWhiteSpace(reusedRefreshToken.ReplacedByTokenHash))
                return;

            var nextTokenHash = reusedRefreshToken.ReplacedByTokenHash;
            var visitedTokenHashes = new HashSet<string>(StringComparer.Ordinal);

            while (!string.IsNullOrWhiteSpace(nextTokenHash) && visitedTokenHashes.Add(nextTokenHash))
            {
                var descendant = await _context.RefreshTokens
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(x => x.TokenHash == nextTokenHash);

                if (descendant == null)
                    break;

                RevokeRefreshToken(descendant, revokedByIp);
                nextTokenHash = descendant.ReplacedByTokenHash;
            }

            await _context.SaveChangesAsync();
        }

        private AuthTokenResponseDto BuildAuthResponse(
            User user,
            string accessToken,
            DateTime accessTokenExpiresAtUtc,
            string refreshToken,
            DateTime refreshTokenExpiresAtUtc)
        {
            return new AuthTokenResponseDto
            {
                Token = accessToken,
                ExpiresAtUtc = accessTokenExpiresAtUtc,
                UserId = user.Id,
                Role = user.Role,
                RefreshToken = refreshToken,
                RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc
            };
        }

        private int? GetAuthenticatedUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idStr, out var userId) ? userId : null;
        }

        private string? GetClientIp()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        private static void RevokeRefreshToken(RefreshToken refreshToken, string? revokedByIp, string? replacedByTokenHash = null)
        {
            refreshToken.RevokedAt ??= DateTime.UtcNow;
            refreshToken.RevokedByIp ??= NormalizeIpAddress(revokedByIp);
            refreshToken.ReplacedByTokenHash ??= replacedByTokenHash;
        }

        private static string? NormalizeIpAddress(string? ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return null;

            var normalized = ipAddress.Trim();
            return normalized.Length <= 64 ? normalized : normalized[..64];
        }
    }
}
