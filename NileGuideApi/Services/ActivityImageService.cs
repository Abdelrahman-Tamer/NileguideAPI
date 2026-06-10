using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace NileGuideApi.Services
{
    public class ActivityImageService : IActivityImageService
    {
        private readonly Cloudinary? _cloudinary;

        public ActivityImageService(IConfiguration configuration)
        {
            var cloudName = configuration["Cloudinary:CloudName"];
            var apiKey = configuration["Cloudinary:ApiKey"];
            var apiSecret = configuration["Cloudinary:ApiSecret"];

            if (!string.IsNullOrWhiteSpace(cloudName) &&
                !string.IsNullOrWhiteSpace(apiKey) &&
                !string.IsNullOrWhiteSpace(apiSecret))
            {
                var account = new Account(cloudName, apiKey, apiSecret);
                _cloudinary = new Cloudinary(account);
                _cloudinary.Api.Secure = true;
            }
        }

        public async Task<List<string>> UploadAsync(List<IFormFile> files, int activityId)
        {
            if (_cloudinary == null)
                throw new InvalidOperationException("Cloudinary is not configured");

            var urls = new List<string>();

            foreach (var file in files)
            {
                if (file == null || file.Length == 0)
                    continue;

                await using var stream = file.OpenReadStream();

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = $"nileguide/activities/{activityId}",
                    PublicId = $"{Guid.NewGuid():N}"
                };

                var result = await _cloudinary.UploadAsync(uploadParams);

                if (result.StatusCode != System.Net.HttpStatusCode.OK &&
                    result.StatusCode != System.Net.HttpStatusCode.Created)
                {
                    throw new InvalidOperationException("Activity image upload failed");
                }

                urls.Add(result.SecureUrl.ToString());
            }

            return urls;
        }

        public async Task DeleteByUrlAsync(string? imageUrl, int activityId)
        {
            if (string.IsNullOrWhiteSpace(imageUrl) || _cloudinary == null)
                return;

            var publicId = ExtractPublicId(imageUrl);
            if (string.IsNullOrWhiteSpace(publicId))
                return;

            await _cloudinary.DestroyAsync(new DeletionParams(publicId));
        }

        public async Task DeleteManyByUrlsAsync(IEnumerable<string> imageUrls, int activityId)
        {
            foreach (var url in imageUrls)
            {
                await DeleteByUrlAsync(url, activityId);
            }
        }

        private static string? ExtractPublicId(string url)
        {
            try
            {
                var uri = new Uri(url);
                var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

                var uploadIndex = Array.FindIndex(segments, x => x.Equals("upload", StringComparison.OrdinalIgnoreCase));
                if (uploadIndex < 0 || uploadIndex + 1 >= segments.Length)
                    return null;

                var remaining = segments.Skip(uploadIndex + 1).ToList();

                if (remaining.Count > 0 && remaining[0].StartsWith("v", StringComparison.OrdinalIgnoreCase))
                    remaining.RemoveAt(0);

                if (remaining.Count == 0)
                    return null;

                var joined = string.Join('/', remaining);
                var dotIndex = joined.LastIndexOf('.');
                return dotIndex > 0 ? joined[..dotIndex] : joined;
            }
            catch
            {
                return null;
            }
        }
    }
}