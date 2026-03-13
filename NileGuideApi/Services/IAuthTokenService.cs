using NileGuideApi.Models;

namespace NileGuideApi.Services
{
    public interface IAuthTokenService
    {
        (string token, DateTime expiresAtUtc) GenerateAccessToken(User user);
        (RefreshToken refreshToken, string plainTextToken) GenerateRefreshToken(User user, bool rememberMe, string? createdByIp);
        string HashRefreshToken(string refreshToken);
        TimeSpan GetRefreshTokenLifetime(bool rememberMe);
        bool UsesRememberMeLifetime(RefreshToken refreshToken);
    }
}
