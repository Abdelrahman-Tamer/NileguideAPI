using Microsoft.AspNetCore.Http;

namespace NileGuideApi.Services
{
    public interface IActivityImageService
    {
        Task<List<string>> UploadAsync(List<IFormFile> files, int activityId);
        Task DeleteByUrlAsync(string? imageUrl, int activityId);
        Task DeleteManyByUrlsAsync(IEnumerable<string> imageUrls, int activityId);
    }
}