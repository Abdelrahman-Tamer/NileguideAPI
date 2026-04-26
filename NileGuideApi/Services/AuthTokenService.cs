using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NileGuideApi.Models;
using NileGuideApi.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace NileGuideApi.Services
{
    // Centralizes access-token creation and secure refresh-token generation.
    public class AuthTokenService : IAuthTokenService
    {
        private readonly JwtOptions _jwtOptions;

        public AuthTokenService(IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
        }

        public (string token, DateTime expiresAtUtc) GenerateAccessToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAtUtc = DateTime.UtcNow.AddMinutes(GetAccessTokenMinutes());

            var jwt = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
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
            var days = rememberMe ? _jwtOptions.RefreshTokenRememberMeDays : _jwtOptions.RefreshTokenDays;

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
            return _jwtOptions.AccessTokenMinutes > 0
                ? _jwtOptions.AccessTokenMinutes
                : _jwtOptions.ExpiryMinutes;
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
