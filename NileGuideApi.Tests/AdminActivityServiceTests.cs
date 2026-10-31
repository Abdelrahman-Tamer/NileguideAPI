using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NileGuideApi.Data;
using NileGuideApi.Hubs;
using NileGuideApi.Models;
using NileGuideApi.Services;
using Xunit;

namespace NileGuideApi.Tests
{
    public class MockAiSyncService : IAiSyncService
    {
        public List<int> SyncedUpserts { get; } = new();
        public List<int> SyncedDeletes { get; } = new();
        public List<string> DeletedConversations { get; } = new();

        public Task SyncActivityAsync(int activityId, string title, string city, string category, decimal price, string currency, string description, string imageUrl, bool isActive, string action = "upsert")
        {
            if (action == "delete" || !isActive)
                SyncedDeletes.Add(activityId);
            else
                SyncedUpserts.Add(activityId);

            return Task.CompletedTask;
        }

        public Task DeleteActivityAsync(int activityId)
        {
            SyncedDeletes.Add(activityId);
            return Task.CompletedTask;
        }

        public Task DeleteConversationHistoryAsync(string conversationId)
        {
            DeletedConversations.Add(conversationId);
            return Task.CompletedTask;
        }
    }

    public class MockActivityImageService : IActivityImageService
    {
        public Task<List<string>> UploadAsync(List<IFormFile> files, int activityId) => Task.FromResult(new List<string>());
        public Task DeleteByUrlAsync(string? imageUrl, int activityId) => Task.CompletedTask;
        public Task DeleteManyByUrlsAsync(IEnumerable<string> imageUrls, int activityId) => Task.CompletedTask;
    }

    public class MockHubClients : IHubClients
    {
        public IClientProxy All => new MockClientProxy();
        public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => new MockClientProxy();
        public IClientProxy Client(string connectionId) => new MockClientProxy();
        public IClientProxy Clients(IReadOnlyList<string> connectionIds) => new MockClientProxy();
        public IClientProxy Group(string groupName) => new MockClientProxy();
        public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => new MockClientProxy();
        public IClientProxy Groups(IReadOnlyList<string> groupNames) => new MockClientProxy();
        public IClientProxy User(string userId) => new MockClientProxy();
        public IClientProxy Users(IReadOnlyList<string> userIds) => new MockClientProxy();
    }

    public class MockClientProxy : IClientProxy
    {
        public Task SendCoreAsync(string method, object?[] args, System.Threading.CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    public class MockHubContext : IHubContext<DashboardHub>
    {
        public IHubClients Clients => new MockHubClients();
        public IGroupManager Groups => throw new NotImplementedException();
    }

    public class AdminActivityServiceTests
    {
        private AppDbContext CreateInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var db = new AppDbContext(options);
            db.Categories.Add(new Category { CategoryID = 1, CategoryName = "Tours" });
            db.Cities.Add(new City { CityID = 1, CityName = "Alexandria" });
            db.SaveChanges();

            return db;
        }

        [Fact]
        public async Task DeleteAsync_ShouldSetIsActiveToFalse_AndCallAiSyncDelete()
        {
            var db = CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var activity = new Activity
            {
                ActivityID = 50,
                CategoryID = 1,
                CityID = 1,
                ActivityName = "Alexandria Day Trip",
                Description = "Visit Alexandria",
                MinPrice = 60,
                IsActive = true,
                DeletedAt = null
            };
            db.Activities.Add(activity);
            await db.SaveChangesAsync();

            var mockAiSync = new MockAiSyncService();
            var mockImageSvc = new MockActivityImageService();
            var mockHub = new MockHubContext();

            var adminService = new AdminActivityService(db, mockImageSvc, mockHub, mockAiSync);
            await adminService.DeleteAsync(50);

            // Use IgnoreQueryFilters because DeletedAt was set by soft delete
            var deletedInDb = await db.Activities.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.ActivityID == 50);
            Assert.NotNull(deletedInDb);
            Assert.False(deletedInDb.IsActive);
            Assert.NotNull(deletedInDb.DeletedAt);

            Assert.Contains(50, mockAiSync.SyncedDeletes);
        }
    }
}
