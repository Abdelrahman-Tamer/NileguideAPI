using Microsoft.EntityFrameworkCore;
using NileGuideApi.Data;
using NileGuideApi.DTOs;

namespace NileGuideApi.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;

        public ReportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ActivityViewsDto>> GetActivityViewsLast7DaysAsync()
        {
            var today = DateTime.UtcNow.Date;
            var startDate = today.AddDays(-6);

            var views = await _context.ActivityViews
                .AsNoTracking()
                .Where(x => x.ViewedAt.Date >= startDate && x.ViewedAt.Date <= today)
                .ToListAsync();

            var result = Enumerable.Range(0, 7)
                .Select(i => startDate.AddDays(i))
                .Select(dayDate => new ActivityViewsDto
                {
                    Day = dayDate.ToString("ddd"),
                    Views = views.Count(x => x.ViewedAt.Date == dayDate)
                })
                .ToList();

            return result;
        }

        public async Task<List<UserGrowthDto>> GetUserGrowthAsync()
        {
            var now = DateTime.UtcNow;
            var startDate = new DateTime(now.Year, now.Month, 1).AddMonths(-5);

            var users = await _context.Users
                .AsNoTracking()
                .Where(x => x.DeletedAt == null && x.CreatedAt >= startDate)
                .ToListAsync();

            var result = Enumerable.Range(0, 6)
                .Select(i => startDate.AddMonths(i))
                .Select(monthDate => new UserGrowthDto
                {
                    Month = monthDate.ToString("MMM"),
                    Count = users.Count(x =>
                        x.CreatedAt.Year == monthDate.Year &&
                        x.CreatedAt.Month == monthDate.Month)
                })
                .ToList();

            return result;
        }

        public async Task<List<ActivitiesByCategoryDto>> GetActivitiesByCategoryAsync()
        {
            var result = await _context.Activities
                .AsNoTracking()
                .Where(x => x.DeletedAt == null)
                .GroupBy(x => x.Category.CategoryName)
                .Select(g => new ActivitiesByCategoryDto
                {
                    Category = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            return result;
        }

        public async Task<List<PopularActivityDto>> GetTopPopularActivitiesAsync(int top = 5)
        {
            var result = await _context.Activities
                .AsNoTracking()
                .Where(x => x.DeletedAt == null)
                .OrderByDescending(x => x.ReviewCount)
                .ThenByDescending(x => x.Rating)
                .Take(top)
                .Select(x => new PopularActivityDto
                {
                    ActivityId = x.ActivityID,
                    ActivityName = x.ActivityName,
                    Value = x.ReviewCount
                })
                .ToListAsync();

            return result;
        }
    }
}