using NileGuideApi.DTOs;

namespace NileGuideApi.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllCategoriesAsync();
    }
}