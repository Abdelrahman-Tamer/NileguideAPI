using NileGuideApi.DTOs;

namespace NileGuideApi.Services
{
    public interface IPlanService
    {
        Task<PlanResponse> GetPlanAsync(int userId);

        Task<PlanItemResponse> AddItemAsync(int userId, AddPlanItemRequest request);

        Task DeleteItemAsync(int userId, int planItemId);

        Task<List<int>> GetActivityIdsAsync(int userId);
    }
}
