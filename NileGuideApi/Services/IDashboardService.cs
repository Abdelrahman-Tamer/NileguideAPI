using NileGuideApi.DTOs;

namespace NileGuideApi.Services
{
    /// <summary>
    /// Provides dashboard values for the admin panel.
    /// </summary>
    public interface IDashboardService
    {
        Task<DashboardDto> GetAsync();
    }
}