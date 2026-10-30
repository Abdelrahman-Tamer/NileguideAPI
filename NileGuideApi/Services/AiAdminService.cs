using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NileGuideApi.Options;

namespace NileGuideApi.Services
{
    public class AiAdminService : IAiAdminService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AiAdminService> _logger;
        private readonly IOptions<MiniRagOptions>? _miniRagOptions;

        public AiAdminService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<AiAdminService> logger,
            IOptions<MiniRagOptions>? miniRagOptions = null)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _miniRagOptions = miniRagOptions;
        }

        private string GetBaseUrl()
        {
            var optionsUrl = _miniRagOptions?.Value?.BaseUrl;
            if (!string.IsNullOrWhiteSpace(optionsUrl))
                return optionsUrl.TrimEnd('/');

            return (_configuration["MiniRag:BaseUrl"]
                ?? _configuration["AiSettings:BaseUrl"]
                ?? "https://mini-rag-alpha.vercel.app").TrimEnd('/');
        }

        private string GetApiKey()
        {
            var optionsKey = _miniRagOptions?.Value?.AdminApiKey ?? _miniRagOptions?.Value?.ApiKey;
            if (!string.IsNullOrWhiteSpace(optionsKey))
                return optionsKey.Trim();

            return (_configuration["MiniRag:AdminApiKey"]
                ?? _configuration["MiniRag:ApiKey"]
                ?? _configuration["AiSettings:AdminApiKey"]
                ?? _configuration["AiSettings:ApiKey"]
                ?? string.Empty).Trim();
        }

        private void AttachAuthHeaders(HttpRequestMessage request)
        {
            var apiKey = GetApiKey();
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                request.Headers.Add("X-Admin-Api-Key", apiKey);
            }
        }

        public async Task<(int StatusCode, string ResponseBody)> GetFilesAsync()
        {
            try
            {
                var url = $"{GetBaseUrl()}/api/v1/data/files";
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                AttachAuthHeaders(request);

                using var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();
                return ((int)response.StatusCode, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while proxying GetFiles request to mini-RAG");
                return (StatusCodes.Status502BadGateway, "{\"message\": \"Failed to communicate with AI knowledge service.\"}");
            }
        }

        public async Task<(int StatusCode, string ResponseBody)> UploadFilesAsync(List<IFormFile> files, int chunkSize, int overlapSize)
        {
            try
            {
                var url = $"{GetBaseUrl()}/api/v1/data/multi-upload";
                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                AttachAuthHeaders(request);

                using var content = new MultipartFormDataContent();
                foreach (var file in files)
                {
                    var streamContent = new StreamContent(file.OpenReadStream());
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
                    content.Add(streamContent, "files", file.FileName);
                }

                content.Add(new StringContent(chunkSize.ToString()), "chunk_size");
                content.Add(new StringContent(overlapSize.ToString()), "overlap_size");

                request.Content = content;

                using var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();
                return ((int)response.StatusCode, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while proxying UploadFiles request to mini-RAG");
                return (StatusCodes.Status502BadGateway, "{\"message\": \"Failed to communicate with AI knowledge service.\"}");
            }
        }

        public async Task<(int StatusCode, string ResponseBody)> UpdateFileAsync(string fileId, IFormFile file, int chunkSize, int overlapSize)
        {
            try
            {
                var url = $"{GetBaseUrl()}/api/v1/data/file/{Uri.EscapeDataString(fileId)}";
                using var request = new HttpRequestMessage(HttpMethod.Put, url);
                AttachAuthHeaders(request);

                using var content = new MultipartFormDataContent();
                var streamContent = new StreamContent(file.OpenReadStream());
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
                content.Add(streamContent, "file", file.FileName);

                content.Add(new StringContent(chunkSize.ToString()), "chunk_size");
                content.Add(new StringContent(overlapSize.ToString()), "overlap_size");

                request.Content = content;

                using var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();
                return ((int)response.StatusCode, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while proxying UpdateFile request to mini-RAG for fileId: {FileId}", fileId);
                return (StatusCodes.Status502BadGateway, "{\"message\": \"Failed to communicate with AI knowledge service.\"}");
            }
        }

        public async Task<(int StatusCode, string ResponseBody)> DeleteFileAsync(string fileId)
        {
            try
            {
                var url = $"{GetBaseUrl()}/api/v1/data/file/{Uri.EscapeDataString(fileId)}";
                using var request = new HttpRequestMessage(HttpMethod.Delete, url);
                AttachAuthHeaders(request);

                using var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();
                return ((int)response.StatusCode, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while proxying DeleteFile request to mini-RAG for fileId: {FileId}", fileId);
                return (StatusCodes.Status502BadGateway, "{\"message\": \"Failed to communicate with AI knowledge service.\"}");
            }
        }
    }
}
