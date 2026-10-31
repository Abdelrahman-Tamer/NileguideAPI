using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NileGuideApi.Options;
using NileGuideApi.Services;
using Xunit;

namespace NileGuideApi.Tests
{
    public class AiSyncServiceTests
    {
        private class TestHttpMessageHandler : HttpMessageHandler
        {
            public HttpRequestMessage? LastRequest { get; private set; }
            public HttpStatusCode ResponseStatusCode { get; set; } = HttpStatusCode.OK;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                LastRequest = request;
                return Task.FromResult(new HttpResponseMessage(ResponseStatusCode)
                {
                    Content = new StringContent("{\"status\":\"ok\"}")
                });
            }
        }

        [Fact]
        public async Task SyncActivityAsync_WhenApiKeyMissing_DoesNotSendRequestWithHardcodedFallback()
        {
            var handler = new TestHttpMessageHandler();
            var httpClient = new HttpClient(handler);
            var configuration = new ConfigurationBuilder().Build();
            var options = Microsoft.Extensions.Options.Options.Create(new MiniRagOptions
            {
                BaseUrl = "https://test-minirag.local",
                ApiKey = null // No key configured
            });

            var service = new AiSyncService(httpClient, configuration, NullLogger<AiSyncService>.Instance, options);

            await service.SyncActivityAsync(101, "Pyramids Tour", "Giza", "History", 50m, "USD", "Great tour", "http://img.local/1.jpg", true);

            // Must NOT have dispatched a request with a fallback secret like "nileguide-admin-secret-2026"
            Assert.Null(handler.LastRequest);
        }

        [Fact]
        public async Task SyncActivityAsync_WhenApiKeyConfigured_SendsCorrectHeaderAndUrl()
        {
            var handler = new TestHttpMessageHandler();
            var httpClient = new HttpClient(handler);
            var configuration = new ConfigurationBuilder().Build();
            var options = Microsoft.Extensions.Options.Options.Create(new MiniRagOptions
            {
                BaseUrl = "https://test-minirag.local",
                ApiKey = "configured-production-secret-987"
            });

            var service = new AiSyncService(httpClient, configuration, NullLogger<AiSyncService>.Instance, options);

            await service.SyncActivityAsync(102, "Karnak Temple", "Luxor", "History", 75m, "USD", "Temple visit", "http://img.local/2.jpg", true);

            Assert.NotNull(handler.LastRequest);
            Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
            Assert.Equal("https://test-minirag.local/api/v1/nlp/sync/activity", handler.LastRequest.RequestUri!.ToString());
            Assert.True(handler.LastRequest.Headers.Contains("X-Admin-Api-Key"));
            Assert.Equal("configured-production-secret-987", handler.LastRequest.Headers.GetValues("X-Admin-Api-Key").First());
        }

        [Fact]
        public async Task DeleteConversationHistoryAsync_WhenApiKeyConfigured_PassesAdminHeader()
        {
            var handler = new TestHttpMessageHandler();
            var httpClient = new HttpClient(handler);
            var configuration = new ConfigurationBuilder().Build();
            var options = Microsoft.Extensions.Options.Options.Create(new MiniRagOptions
            {
                BaseUrl = "https://test-minirag.local",
                ApiKey = "configured-production-secret-987"
            });

            var service = new AiSyncService(httpClient, configuration, NullLogger<AiSyncService>.Instance, options);

            await service.DeleteConversationHistoryAsync("conv-12345");

            Assert.NotNull(handler.LastRequest);
            Assert.Equal(HttpMethod.Delete, handler.LastRequest!.Method);
            Assert.Equal("https://test-minirag.local/api/v1/nlp/conversation/conv-12345", handler.LastRequest.RequestUri!.ToString());
            Assert.True(handler.LastRequest.Headers.Contains("X-Admin-Api-Key"));
            Assert.Equal("configured-production-secret-987", handler.LastRequest.Headers.GetValues("X-Admin-Api-Key").First());
        }
    }
}
