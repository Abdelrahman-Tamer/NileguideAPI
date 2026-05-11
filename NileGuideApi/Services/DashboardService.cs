using Microsoft.EntityFrameworkCore;
using NileGuideApi.Data;
using NileGuideApi.DTOs;

namespace NileGuideApi.Services
{
    /// <summary>
    /// Reads dashboard card values directly from the database.
    /// </summary>
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardDto> GetAsync()
        {
            var totalUsers = await _context.Users
                .AsNoTracking()
                .CountAsync(x => x.IsActive && x.DeletedAt == null);

            var totalActivities = await _context.Activities
                .AsNoTracking()
                .CountAsync(x => x.DeletedAt == null);

            var totalReviews = await _context.Reviews
                .AsNoTracking()
                .CountAsync(x => x.DeletedAt == null);

            var wishlistItems = await _context.WishlistItems
                .AsNoTracking()
                .CountAsync();

            var averageRating = await _context.Activities
                .AsNoTracking()
                .Where(x => x.DeletedAt == null && x.ReviewCount > 0)
                .Select(x => (double?)x.Rating)
                .AverageAsync();

            return new DashboardDto
            {
                TotalUsers = totalUsers,
                TotalActivities = totalActivities,
                TotalReviews = totalReviews,
                WishlistItems = wishlistItems,
                AverageRating = averageRating.HasValue
                    ? Math.Round(averageRating.Value, 1)
                    : 0
            };
        }
    }
}