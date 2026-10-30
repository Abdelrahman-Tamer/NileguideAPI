using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NileGuideApi.Controllers;
using NileGuideApi.Services;
using Xunit;

namespace NileGuideApi.Tests
{
    public class MockAiAdminService : IAiAdminService
    {
        public bool GetFilesCalled { get; private set; }
        public bool UploadFilesCalled { get; private set; }
        public bool DeleteFileCalled { get; private set; }
        public string? LastDeletedFileId { get; private set; }

        public int ReturnStatusCode { get; set; } = StatusCodes.Status200OK;
        public string ReturnResponseBody { get; set; } = "{\"assets\":[]}";

        public Task<(int StatusCode, string ResponseBody)> GetFilesAsync()
        {
            GetFilesCalled = true;
            return Task.FromResult((ReturnStatusCode, ReturnResponseBody));
        }

        public Task<(int StatusCode, string ResponseBody)> UploadFilesAsync(List<IFormFile> files, int chunkSize, int overlapSize)
        {
            UploadFilesCalled = true;
            return Task.FromResult((ReturnStatusCode, ReturnResponseBody));
        }

        public Task<(int StatusCode, string ResponseBody)> UpdateFileAsync(string fileId, IFormFile file, int chunkSize, int overlapSize)
        {
            return Task.FromResult((ReturnStatusCode, ReturnResponseBody));
        }

        public Task<(int StatusCode, string ResponseBody)> DeleteFileAsync(string fileId)
        {
            DeleteFileCalled = true;
            LastDeletedFileId = fileId;
            return Task.FromResult((ReturnStatusCode, ReturnResponseBody));
        }
    }

    public class AdminAiControllerTests
    {
        [Fact]
        public void Controller_MustHaveAdminOnlyPolicyAttribute()
        {
            var authAttribute = typeof(AdminAiController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .FirstOrDefault() as AuthorizeAttribute;

            Assert.NotNull(authAttribute);
            Assert.Equal("AdminOnly", authAttribute.Policy);
        }

        [Fact]
        public async Task GetFiles_ShouldDelegateToAiAdminService_AndReturnContentResult()
        {
            var mockService = new MockAiAdminService
            {
                ReturnResponseBody = "{\"assets\":[{\"file_id\":\"f1\",\"asset_name\":\"guide.txt\"}]}"
            };
            var controller = new AdminAiController(mockService);

            var actionResult = await controller.GetFiles();
            var contentResult = Assert.IsType<ContentResult>(actionResult);

            Assert.True(mockService.GetFilesCalled);
            Assert.Equal("application/json", contentResult.ContentType);
            Assert.Contains("guide.txt", contentResult.Content);
        }

        [Fact]
        public async Task UploadFiles_ShouldRejectEmptyFileList_WithBadRequest()
        {
            var mockService = new MockAiAdminService();
            var controller = new AdminAiController(mockService);

            var result = await controller.UploadFiles(new List<IFormFile>());
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.False(mockService.UploadFilesCalled);
        }

        [Fact]
        public async Task DeleteFile_ShouldDelegateToAiAdminService()
        {
            var mockService = new MockAiAdminService();
            var controller = new AdminAiController(mockService);

            var result = await controller.DeleteFile("file_abc123");
            Assert.True(mockService.DeleteFileCalled);
            Assert.Equal("file_abc123", mockService.LastDeletedFileId);
        }

        [Fact]
        public async Task GetFiles_WhenRagServiceIsUnreachable_ShouldReturn503Gracefully()
        {
            var mockService = new MockAiAdminService
            {
                ReturnStatusCode = StatusCodes.Status503ServiceUnavailable,
                ReturnResponseBody = "{\"error\":\"AI RAG service is temporarily unreachable.\"}"
            };
            var controller = new AdminAiController(mockService);

            var actionResult = await controller.GetFiles();
            var contentResult = Assert.IsType<ContentResult>(actionResult);
            Assert.Contains("AI RAG service is temporarily unreachable", contentResult.Content);
        }
    }
}
