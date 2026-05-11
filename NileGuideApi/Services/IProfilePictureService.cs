using Microsoft.AspNetCore.Http;

namespace NileGuideApi.Services
{
    public interface IProfilePictureService
    {
        Task<string> UploadAsync(IFormFile image, int userId);
        Task DeleteByUrlAsync(string? imageUrl, int userId);
    }
}
