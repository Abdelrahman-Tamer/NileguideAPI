using NileGuideApi.DTOs;

namespace NileGuideApi.Services
{
    public interface IAdminActivityService
    {
        Task<AdminActivityDetailsDto> CreateAsync(CreateActivityDto dto);
        Task<AdminActivityDetailsDto> UpdateAsync(int id, UpdateActivityDto dto);
        Task DeleteAsync(int id);
    }
}