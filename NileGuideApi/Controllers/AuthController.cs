using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NileGuideApi.Data;
using NileGuideApi.DTOs;
using NileGuideApi.Models;
using NileGuideApi.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace NileGuideApi.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IEmailSender _emailSender;

        public AuthController(AppDbContext context, IConfiguration config, IEmailSender emailSender)
        {
            _context = context;
            _config = config;
            _emailSender = emailSender;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var email = (dto.Email ?? "").Trim().ToLowerInvariant();

            // Email is stored normalized (lowercase) => compare directly to keep index usable
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
            await _context.SaveChangesAsync();

            var (token, expiresAtUtc) = GenerateJwt(user);
            return Ok(new { token, expiresAtUtc, userId = user.Id, role = user.Role });
        }

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

            var (token, expiresAtUtc) = GenerateJwt(user);
            return Ok(new { token, expiresAtUtc, userId = user.Id, role = user.Role });
        }

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

            // اقفل على blocked/deleted
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

        [EnableRateLimiting("ResetPolicy")]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var email = (dto.Email ?? "").Trim().ToLowerInvariant();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            // Always OK (no enumeration)
            if (user == null || !user.IsActive || user.DeletedAt != null)
                return Ok(new { message = "If the email exists, a reset code was sent." });

            // Invalidate previous active tokens
            var activeTokens = await _context.PasswordResetTokens
                .Where(t => t.UserId == user.Id && t.UsedAt == null && t.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();

            foreach (var t in activeTokens)
                t.UsedAt = DateTime.UtcNow;

            var code = Generate6DigitCode();
            var tokenHash = HashResetCode(user.Id, code);

            var token = new PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                AttemptCount = 0
            };

            _context.PasswordResetTokens.Add(token);
            await _context.SaveChangesAsync();

            await _emailSender.SendEmailAsync(
                user.Email,
                "NileGuide Password Reset Code",
                $"Your reset code is: {code}\nThis code expires in 10 minutes.");

            return Ok(new { message = "If the email exists, a reset code was sent." });
        }

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

            // نجاح: ما تزودش AttemptCount
            return Ok(new { message = "Code is valid" });
        }

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

            // منع نفس الباسورد القديم
            if (BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.PasswordHash))
                return BadRequest(new { message = "New password cannot be the same as the old password" });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            token.UsedAt = DateTime.UtcNow;
            token.LastAttemptAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Password updated" });
        }

        private (string token, DateTime expiresAtUtc) GenerateJwt(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var keyStr = _config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var mins = 30.0;
            if (double.TryParse(_config["Jwt:ExpiryMinutes"], out var parsed)) mins = parsed;

            var expiresAtUtc = DateTime.UtcNow.AddMinutes(mins);

            var jwt = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expiresAtUtc,
                signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(jwt), expiresAtUtc);
        }

        private string Generate6DigitCode()
        {
            var n = RandomNumberGenerator.GetInt32(0, 1_000_000);
            return n.ToString("D6");
        }

        private string HashResetCode(int userId, string code)
        {
            var pepper = _config["Jwt:Key"] ?? "pepper";
            var raw = $"{userId}:{code}:{pepper}";
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
            return Convert.ToHexString(bytes);
        }

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
    }
}
