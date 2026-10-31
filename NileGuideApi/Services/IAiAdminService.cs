using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NileGuideApi.Services
{
    public interface IAiAdminService
    {
        Task<(int StatusCode, string ResponseBody)> GetFilesAsync();
        Task<(int StatusCode, string ResponseBody)> UploadFilesAsync(List<IFormFile> files, int chunkSize, int overlapSize);
        Task<(int StatusCode, string ResponseBody)> UpdateFileAsync(string fileId, IFormFile file, int chunkSize, int overlapSize);
        Task<(int StatusCode, string ResponseBody)> DeleteFileAsync(string fileId);
    }
}
