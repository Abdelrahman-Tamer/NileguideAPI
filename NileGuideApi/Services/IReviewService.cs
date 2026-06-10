using NileGuideApi.DTOs;

namespace NileGuideApi.Services
{
    public interface IReviewService
    {
        Task<List<ReviewDto>> GetAllByActivityIdAsync(int activityId);
        Task<ReviewDto> CreateAsync(int activityId, int userId, CreateReviewDto dto);
    }
}