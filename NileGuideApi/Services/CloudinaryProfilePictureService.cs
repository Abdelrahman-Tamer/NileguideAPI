using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NileGuideApi.Options;

namespace NileGuideApi.Services
{
    public class CloudinaryProfilePictureService : IProfilePictureService
    {
        private const string ProfilePictureFolder = "users/profile_pictures";

        private readonly CloudinaryOptions _options;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CloudinaryProfilePictureService> _logger;

        public CloudinaryProfilePictureService(
            IOptions<CloudinaryOptions> options,
            IConfiguration configuration,
            ILogger<CloudinaryProfilePictureService> logger)
        {
            _options = options.Value;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> UploadAsync(IFormFile image, int userId)
        {
            var cloudinary = CreateCloudinary();

            await using var stream = image.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(image.FileName, stream),
                PublicId = GetProfilePicturePublicId(userId),
                AssetFolder = ProfilePictureFolder,
                DisplayName = $"user_{userId}",
                UseFilename = false,
                UniqueFilename = false,
                Overwrite = true,
                Invalidate = true
            };

            var result = await cloudinary.UploadAsync(uploadParams);

            if (result.Error != null || result.SecureUrl == null)
            {
                _logger.LogWarning("Cloudinary profile picture upload failed: {Error}", result.Error?.Message);
                throw new InvalidOperationException("Profile picture upload failed");
            }

            await MoveToProfilePicturesFolderAsync(cloudinary, result.PublicId, userId);

            return result.SecureUrl.ToString();
        }

        public async Task DeleteByUrlAsync(string? imageUrl, int userId)
        {
            var publicId = TryGetPublicIdFromUrl(imageUrl);
            if (string.IsNullOrWhiteSpace(publicId))
                return;

            if (string.Equals(publicId, GetProfilePicturePublicId(userId), StringComparison.Ordinal))
                return;

            try
            {
                var cloudinary = CreateCloudinary();
                var result = await cloudinary.DestroyAsync(new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Image,
                    Invalidate = true
                });

                if (!string.Equals(result.Result, "ok", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(result.Result, "not found", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Cloudinary old profile picture deletion returned {Result} for public id {PublicId}", result.Result, publicId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cloudinary old profile picture deletion failed for public id {PublicId}", publicId);
            }
        }

        private Cloudinary CreateCloudinary()
        {
            var cloudName = GetConfiguredValue(_options.CloudName, "Cloudinary:CloudName", "CLOUDINARY_CLOUD_NAME");
            var apiKey = GetConfiguredValue(_options.ApiKey, "Cloudinary:ApiKey", "CLOUDINARY_API_KEY");
            var apiSecret = GetConfiguredValue(_options.ApiSecret, "Cloudinary:ApiSecret", "CLOUDINARY_API_SECRET");

            if (string.IsNullOrWhiteSpace(cloudName) ||
                string.IsNullOrWhiteSpace(apiKey) ||
                string.IsNullOrWhiteSpace(apiSecret))
            {
                throw new InvalidOperationException("Cloudinary is not configured");
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            return new Cloudinary(account);
        }

        private static string GetProfilePicturePublicId(int userId)
        {
            return $"{ProfilePictureFolder}/user_{userId}";
        }

        private async Task MoveToProfilePicturesFolderAsync(Cloudinary cloudinary, string publicId, int userId)
        {
            try
            {
                await cloudinary.UpdateResourceAsync(new UpdateParams(publicId)
                {
                    ResourceType = ResourceType.Image,
                    AssetFolder = ProfilePictureFolder,
                    DisplayName = $"user_{userId}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cloudinary profile picture folder update failed for public id {PublicId}", publicId);
            }
        }

        private static string? TryGetPublicIdFromUrl(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl) ||
                !Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri) ||
                !uri.Host.EndsWith("res.cloudinary.com", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var segments = uri.AbsolutePath
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            var uploadIndex = segments.FindIndex(s => string.Equals(s, "upload", StringComparison.OrdinalIgnoreCase));
            if (uploadIndex < 0 || uploadIndex >= segments.Count - 1)
                return null;

            var publicIdSegments = segments.Skip(uploadIndex + 1).ToList();
            if (publicIdSegments.Count > 0 &&
                publicIdSegments[0].Length > 1 &&
                publicIdSegments[0][0] == 'v' &&
                publicIdSegments[0].Skip(1).All(char.IsDigit))
            {
                publicIdSegments.RemoveAt(0);
            }

            if (publicIdSegments.Count == 0)
                return null;

            var lastSegment = publicIdSegments[^1];
            var extension = Path.GetExtension(lastSegment);
            if (!string.IsNullOrWhiteSpace(extension))
                publicIdSegments[^1] = lastSegment[..^extension.Length];

            return string.Join("/", publicIdSegments);
        }

        private string? GetConfiguredValue(string? optionsValue, params string[] keys)
        {
            if (!string.IsNullOrWhiteSpace(optionsValue))
                return optionsValue;

            foreach (var key in keys)
            {
                var value = _configuration[key];
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }

            return null;
        }
    }
}
