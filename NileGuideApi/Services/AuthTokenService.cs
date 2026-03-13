using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using NileGuideApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace NileGuideApi.Services
{
    // Centralizes access-token creation and secure refresh-token generation.
    public class AuthTokenService : IAuthTokenService
    {
        private readonly IConfiguration _config;

        public AuthTokenService(IConfiguration config)
        {
            _config = config;
        }

        public (string token, DateTime expiresAtUtc) GenerateAccessToken(User user)
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
            var expiresAtUtc = DateTime.UtcNow.AddMinutes(GetAccessTokenMinutes());

            var jwt = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expiresAtUtc,
                signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(jwt), expiresAtUtc);
        }

        public (RefreshToken refreshToken, string plainTextToken) GenerateRefreshToken(User user, bool rememberMe, string? createdByIp)
        {
            var now = DateTime.UtcNow;
            var plainTextToken = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));

            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = HashRefreshToken(plainTextToken),
                ExpiresAt = now.Add(GetRefreshTokenLifetime(rememberMe)),
                CreatedAt = now,
                CreatedByIp = NormalizeIpAddress(createdByIp)
            };

            return (refreshToken, plainTextToken);
        }

        public string HashRefreshToken(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new InvalidOperationException("Refresh token is missing");

            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken.Trim()));
            return Convert.ToHexString(bytes);
        }

        public TimeSpan GetRefreshTokenLifetime(bool rememberMe)
        {
            var days = rememberMe
                ? GetPositiveDouble("Jwt:RefreshTokenRememberMeDays", 30)
                : GetPositiveDouble("Jwt:RefreshTokenDays", 1);

            return TimeSpan.FromDays(days);
        }

        public bool UsesRememberMeLifetime(RefreshToken refreshToken)
        {
            var actualLifetime = refreshToken.ExpiresAt - refreshToken.CreatedAt;
            var standardLifetime = GetRefreshTokenLifetime(false);
            var rememberedLifetime = GetRefreshTokenLifetime(true);

            var standardDistance = Math.Abs((actualLifetime - standardLifetime).TotalSeconds);
            var rememberedDistance = Math.Abs((actualLifetime - rememberedLifetime).TotalSeconds);

            return rememberedDistance < standardDistance;
        }

        private double GetAccessTokenMinutes()
        {
            var minutes = GetPositiveDouble("Jwt:AccessTokenMinutes", 0);
            if (minutes > 0)
                return minutes;

            return GetPositiveDouble("Jwt:ExpiryMinutes", 30);
        }

        private double GetPositiveDouble(string key, double fallback)
        {
            return double.TryParse(_config[key], out var parsed) && parsed > 0
                ? parsed
                : fallback;
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
