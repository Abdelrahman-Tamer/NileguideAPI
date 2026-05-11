using NileGuideApi.DTOs;

namespace NileGuideApi.Services
{
    public enum WishlistAddResult
    {
        Added,
        AlreadyExists,
        ActivityNotFound
    }

    public interface IWishlistService
    {
        Task<PagedResultDto<ActivityCardDto>> GetWishlistAsync(int userId, WishlistFilterDto filter);

        Task<List<int>> GetActivityIdsAsync(int userId);

        Task<WishlistAddResult> AddAsync(int userId, int activityId);

        Task RemoveAsync(int userId, int activityId);

        Task<WishlistStatusDto?> GetStatusAsync(int userId, int activityId);
    }
}
