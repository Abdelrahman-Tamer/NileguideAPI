using Microsoft.AspNetCore.Http;

namespace NileGuideApi.DTOs
{
    public class ProfilePictureUploadRequest
    {
        public IFormFile? Image { get; set; }
    }
}
