using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NileGuideApi.Data;
using NileGuideApi.DTOs;
using NileGuideApi.Models;
using NileGuideApi.Services;
using Xunit;

namespace NileGuideApi.Tests
{
    public class ActivityServiceTests
    {
        private AppDbContext CreateInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var db = new AppDbContext(options);

            // Seed default category and city to satisfy foreign key relationships
            db.Categories.Add(new Category { CategoryID = 1, CategoryName = "Tours" });
            db.Cities.Add(new City { CityID = 1, CityName = "Cairo" });
            db.SaveChanges();

            return db;
        }

        [Fact]
        public async Task GetActivitiesAsync_ShouldNotReturnSoftDeletedActivities()
        {
            var db = CreateInMemoryDbContext(Guid.NewGuid().ToString());

            // 1 active, non-deleted activity
            db.Activities.Add(new Activity
            {
                ActivityID = 1,
                CategoryID = 1,
                CityID = 1,
                ActivityName = "Pyramids Tour",
                Description = "Giza Pyramids",
                MinPrice = 50,
                IsActive = true,
                DeletedAt = null
            });

            // 1 soft-deleted activity (DeletedAt set)
            db.Activities.Add(new Activity
            {
                ActivityID = 2,
                CategoryID = 1,
                CityID = 1,
                ActivityName = "Nile Cruise Cancelled",
                Description = "Old cruise",
                MinPrice = 100,
                IsActive = true,
                DeletedAt = DateTime.UtcNow
            });

            // 1 inactive activity
            db.Activities.Add(new Activity
            {
                ActivityID = 3,
                CategoryID = 1,
                CityID = 1,
                ActivityName = "Desert Safari Inactive",
                Description = "Inactive",
                MinPrice = 75,
                IsActive = false,
                DeletedAt = null
            });

            await db.SaveChangesAsync();

            var service = new ActivityService(db);
            var result = await service.GetActivitiesAsync(new ActivityFilterDto { Page = 1, PageSize = 10 });

            Assert.Single(result.Items);
            Assert.Equal(1, result.Items[0].ActivityID);
            Assert.Equal("Pyramids Tour", result.Items[0].ActivityName);
        }

        [Fact]
        public async Task GetActivityByIdAsync_ShouldReturnNull_WhenActivityIsSoftDeleted()
        {
            var db = CreateInMemoryDbContext(Guid.NewGuid().ToString());

            db.Activities.Add(new Activity
            {
                ActivityID = 10,
                CategoryID = 1,
                CityID = 1,
                ActivityName = "Luxor Temple",
                Description = "Historic tour",
                MinPrice = 40,
                IsActive = true,
                DeletedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var service = new ActivityService(db);
            var result = await service.GetActivityByIdAsync(10);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetActivityByIdAsync_ShouldNotCreateActivityView_ReadOperationMustBeIdempotent()
        {
            var db = CreateInMemoryDbContext(Guid.NewGuid().ToString());

            db.Activities.Add(new Activity
            {
                ActivityID = 20,
                CategoryID = 1,
                CityID = 1,
                ActivityName = "Aswan Felucca",
                Description = "Felucca sailing",
                MinPrice = 30,
                IsActive = true,
                DeletedAt = null
            });
            await db.SaveChangesAsync();

            var service = new ActivityService(db);
            var result = await service.GetActivityByIdAsync(20);

            Assert.NotNull(result);

            // Verify no ActivityView was created
            var viewsCount = await db.ActivityViews.CountAsync();
            Assert.Equal(0, viewsCount);
        }

        [Fact]
        public async Task RecordViewAsync_ShouldExplicitlyCreateActivityView()
        {
            var db = CreateInMemoryDbContext(Guid.NewGuid().ToString());

            db.Activities.Add(new Activity
            {
                ActivityID = 30,
                CategoryID = 1,
                CityID = 1,
                ActivityName = "Khan El Khalili",
                Description = "Bazaar walking tour",
                MinPrice = 20,
                IsActive = true,
                DeletedAt = null
            });
            await db.SaveChangesAsync();

            var service = new ActivityService(db);
            var success = await service.RecordViewAsync(30);

            Assert.True(success);

            var views = await db.ActivityViews.ToListAsync();
            Assert.Single(views);
            Assert.Equal(30, views[0].ActivityId);
        }

        [Fact]
        public async Task GetActivitiesBatchAsync_ShouldReturnOnlyActiveAndNonDeletedActivities()
        {
            var db = CreateInMemoryDbContext(Guid.NewGuid().ToString());

            db.Activities.AddRange(new[]
            {
                new Activity { ActivityID = 101, CategoryID = 1, CityID = 1, ActivityName = "Activity 101", IsActive = true, DeletedAt = null },
                new Activity { ActivityID = 102, CategoryID = 1, CityID = 1, ActivityName = "Activity 102", IsActive = false, DeletedAt = null },
                new Activity { ActivityID = 103, CategoryID = 1, CityID = 1, ActivityName = "Activity 103", IsActive = true, DeletedAt = DateTime.UtcNow },
                new Activity { ActivityID = 104, CategoryID = 1, CityID = 1, ActivityName = "Activity 104", IsActive = true, DeletedAt = null }
            });
            await db.SaveChangesAsync();

            var service = new ActivityService(db);
            var batch = await service.GetActivitiesBatchAsync(new List<int> { 101, 102, 103, 104, 999 });

            Assert.Equal(2, batch.Count);
            Assert.Contains(batch, x => x.ActivityID == 101);
            Assert.Contains(batch, x => x.ActivityID == 104);
        }
    }
}
