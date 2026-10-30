using NileGuideApi.DTOs;

namespace NileGuideApi.Services
{
    public interface IActivityService
    {
        Task<PagedResultDto<ActivityCardDto>> GetActivitiesAsync(ActivityFilterDto filter);

        Task<ActivityDetailsDto?> GetActivityByIdAsync(int id);

        Task<List<ActivityCardDto>> GetActivitiesBatchAsync(List<int> ids);

        Task<bool> RecordViewAsync(int id);
    }
}
