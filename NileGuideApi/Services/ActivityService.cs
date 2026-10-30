using Microsoft.EntityFrameworkCore;
using NileGuideApi.Data;
using NileGuideApi.DTOs;
using NileGuideApi.Models;

namespace NileGuideApi.Services
{
    public class ActivityService : IActivityService
    {
        private readonly AppDbContext _context;

        public ActivityService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResultDto<ActivityCardDto>> GetActivitiesAsync(ActivityFilterDto filter)
        {
            if (filter.Page <= 0)
                filter.Page = 1;

            if (filter.PageSize <= 0)
                filter.PageSize = 9;

            var query = _context.Activities
                .AsNoTracking()
                .Where(a => a.IsActive && a.DeletedAt == null)
                .Include(a => a.Category)
                .Include(a => a.City)
                .Include(a => a.ActivityImages)
                .Include(a => a.BookingLinks)
                .Include(a => a.ActivityHours)
                .AsSplitQuery()
                .AsQueryable();

            if (filter.CategoryIds != null && filter.CategoryIds.Any())
            {
                var categoryIds = filter.CategoryIds.Distinct().ToList();
                query = query.Where(a => categoryIds.Contains(a.CategoryID));
            }

            if (filter.CityIds != null && filter.CityIds.Any())
            {
                var cityIds = filter.CityIds.Distinct().ToList();
                query = query.Where(a => cityIds.Contains(a.CityID));
            }

            var search = filter.Search?.Trim();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(a =>
                    a.ActivityName.Contains(search) ||
                    (a.Description != null && a.Description.Contains(search))
                );
            }

            query = (filter.SortBy ?? "default").Trim().ToLowerInvariant() switch
            {
                "pricelowtohigh" => query.OrderBy(a => a.MinPrice),
                "pricehightolow" => query.OrderByDescending(a => a.MinPrice),
                "name" => query.OrderBy(a => a.ActivityName),
                _ => query.OrderBy(a => a.ActivityID)
            };

            var totalCount = await query.CountAsync();

            var pageActivities = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var activities = pageActivities
                .Select(ActivityDtoMapper.ToCardDto)
                .ToList();

            return new PagedResultDto<ActivityCardDto>
            {
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                Items = activities
            };
        }

        public async Task<ActivityDetailsDto?> GetActivityByIdAsync(int id)
        {
            var activity = await _context.Activities
                .AsNoTracking()
                .Include(a => a.Category)
                .Include(a => a.City)
                .Include(a => a.ActivityImages)
                .Include(a => a.BookingLinks)
                .Include(a => a.ActivityHours)
                .AsSplitQuery()
                .FirstOrDefaultAsync(a => a.ActivityID == id && a.IsActive && a.DeletedAt == null);

            if (activity == null)
                return null;

            return ActivityDtoMapper.ToDetailsDto(activity);
        }

        public async Task<List<ActivityCardDto>> GetActivitiesBatchAsync(List<int> ids)
        {
            if (ids == null || !ids.Any())
                return new List<ActivityCardDto>();

            var distinctIds = ids.Distinct().Take(100).ToList();

            var activities = await _context.Activities
                .AsNoTracking()
                .Where(a => distinctIds.Contains(a.ActivityID) && a.IsActive && a.DeletedAt == null)
                .Include(a => a.Category)
                .Include(a => a.City)
                .Include(a => a.ActivityImages)
                .Include(a => a.BookingLinks)
                .Include(a => a.ActivityHours)
                .AsSplitQuery()
                .ToListAsync();

            return activities
                .Select(ActivityDtoMapper.ToCardDto)
                .ToList();
        }

        public async Task<bool> RecordViewAsync(int activityId)
        {
            var exists = await _context.Activities
                .AnyAsync(a => a.ActivityID == activityId && a.IsActive && a.DeletedAt == null);

            if (!exists)
                return false;

            var activityView = new ActivityView
            {
                ActivityId = activityId,
                ViewedAt = DateTime.UtcNow
            };

            _context.ActivityViews.Add(activityView);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
