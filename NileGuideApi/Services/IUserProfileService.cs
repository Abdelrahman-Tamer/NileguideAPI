using NileGuideApi.DTOs;

namespace NileGuideApi.Services
{
    public interface IUserProfileService
    {
        Task<UserProfileResponseDto?> GetMyProfileAsync(int userId);

        Task<UserProfileResponseDto?> UpdateMyProfileAsync(int userId, UpdateUserProfileDto dto);
    }
}