using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NileGuideApi.Data;
using NileGuideApi.DTOs;
using NileGuideApi.Models;
using NileGuideApi.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NileGuideApi.Controllers
{
    [Route("api/[controller]")]
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
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Email already exists");

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName,
                Nationality = dto.Nationality,
                Role = "Tourist"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok("Registered successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid emali or password");

            if (user.DeletedAt != null || !user.IsActive)
                return Unauthorized("Account is inactive or deleted");

            var token = GenerateJwtToken(user);
            return Ok(new
            {
                Token = token,
                Role = user.Role,
                UserId = user.Id,
                FullName = user.FullName,
                Nationality = user.Nationality
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || user.DeletedAt != null || !user.IsActive) return Ok("If email exists, OTP sent");

            var otp = new Random().Next(100000, 999999).ToString();
            var otpHash = BCrypt.Net.BCrypt.HashPassword(otp);

            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = otpHash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                AttemptCount = 0
            };

            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();

            await _emailSender.SendEmailAsync(user.Email, "NileGuide Password Reset OTP", $"Your OTP is {otp}. It expires in 10 minutes.");

            return Ok("OTP sent to email");
        }

        [HttpPost("verify-reset-code")]
        public async Task<IActionResult> VerifyResetCode(VerifyResetCodeDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return BadRequest("Invalid request");

            var resetToken = await _context.PasswordResetTokens
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync(r => r.UserId == user.Id && r.UsedAt == null && r.ExpiresAt > DateTime.UtcNow);

            if (resetToken == null) return BadRequest("Invalid or expired code");

            resetToken.LastAttemptAt = DateTime.UtcNow;
            resetToken.AttemptCount++;
            if (resetToken.AttemptCount > 5) return BadRequest("Too many attempts");

            if (!BCrypt.Net.BCrypt.Verify(dto.Code, resetToken.TokenHash))
            {
                await _context.SaveChangesAsync();
                return BadRequest("Invalid code");
            }

            return Ok("Code verified");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return BadRequest("Invalid request");

            var resetToken = await _context.PasswordResetTokens
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync(r => r.UserId == user.Id && r.UsedAt == null && r.ExpiresAt > DateTime.UtcNow);

            if (resetToken == null || !BCrypt.Net.BCrypt.Verify(dto.Code, resetToken.TokenHash))
                return BadRequest("Invalid or expired code");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            resetToken.UsedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok("Password reset successfully");
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse(_config["Jwt:ExpiryMinutes"] ?? "60")),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}