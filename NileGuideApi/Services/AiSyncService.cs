using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NileGuideApi.Options;

namespace NileGuideApi.Services
{
    public class AiSyncService : IAiSyncService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AiSyncService> _logger;
        private readonly IOptions<MiniRagOptions>? _miniRagOptions;

        public AiSyncService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<AiSyncService> logger,
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

        public async Task SyncActivityAsync(int activityId, string title, string city, string category, decimal price, string currency, string description, string imageUrl, bool isActive, string action = "upsert")
        {
            try
            {
                var baseUrl = GetBaseUrl();
                var apiKey = GetApiKey();

                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    _logger.LogWarning("MiniRag:ApiKey is not configured. Omitting administrative sync for activity {ActivityId} to prevent unauthorized calls.", activityId);
                    return;
                }

                var payload = new
                {
                    activity_id = activityId,
                    title = title ?? string.Empty,
                    city = city ?? "Egypt",
                    category = category ?? "General",
                    price = $"{price} {currency ?? "USD"}",
                    description = description ?? string.Empty,
                    image_url = imageUrl ?? string.Empty,
                    link = $"https://www.nileguide.online/activities/{activityId}",
                    is_active = isActive,
                    action = action
                };

                var json = JsonSerializer.Serialize(payload);
                var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/v1/nlp/sync/activity")
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
                request.Headers.Add("X-Admin-Api-Key", apiKey);

                using var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to sync activity {ActivityId} to mini-RAG. Status: {StatusCode}, Body: {Error}", activityId, response.StatusCode, error);
                }
                else
                {
                    _logger.LogInformation("Successfully synced activity {ActivityId} to mini-RAG with action '{Action}'", activityId, action);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Transient error while sending sync payload to mini-RAG for activity {ActivityId}", activityId);
            }
        }

        public async Task DeleteActivityAsync(int activityId)
        {
            await SyncActivityAsync(activityId, string.Empty, string.Empty, string.Empty, 0, string.Empty, string.Empty, string.Empty, false, "delete");
        }

        public async Task DeleteConversationHistoryAsync(string conversationId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(conversationId))
                    return;

                var baseUrl = GetBaseUrl();
                var apiKey = GetApiKey();

                var request = new HttpRequestMessage(HttpMethod.Delete, $"{baseUrl}/api/v1/nlp/conversation/{Uri.EscapeDataString(conversationId)}");
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    request.Headers.Add("X-Admin-Api-Key", apiKey);
                }

                using var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully deleted conversation {ConversationId} in mini-RAG MongoDB", conversationId);
                }
                else
                {
                    _logger.LogWarning("Failed to delete conversation history {ConversationId} from mini-RAG. Status: {StatusCode}", conversationId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not delete conversation {ConversationId} from mini-RAG", conversationId);
            }
        }
    }
}
