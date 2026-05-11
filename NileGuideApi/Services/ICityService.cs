using NileGuideApi.DTOs;

namespace NileGuideApi.Services
{
    public interface ICityService
    {
        Task<List<CityDto>> GetAllCitiesAsync();
    }
}