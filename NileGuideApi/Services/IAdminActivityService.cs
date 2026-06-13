using NileGuideApi.DTOs;

namespace NileGuideApi.Services
{
    public interface IAdminActivityService
    {
        Task<PagedResultDto<AdminActivityDetailsDto>> GetAllAsync(ActivityFilterDto filter);
        Task<AdminActivityDetailsDto> CreateAsync(CreateActivityDto dto);
        Task<AdminActivityDetailsDto> UpdateAsync(int id, UpdateActivityDto dto);
        Task DeleteAsync(int id);
    }
}
