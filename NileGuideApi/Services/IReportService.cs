using NileGuideApi.DTOs;

namespace NileGuideApi.Services
{
    public interface IReportService
    {
        Task<List<ActivityViewsDto>> GetActivityViewsLast7DaysAsync();
        Task<List<UserGrowthDto>> GetUserGrowthAsync();
        Task<List<ActivitiesByCategoryDto>> GetActivitiesByCategoryAsync();
        Task<List<PopularActivityDto>> GetTopPopularActivitiesAsync(int top = 5);
    }
}