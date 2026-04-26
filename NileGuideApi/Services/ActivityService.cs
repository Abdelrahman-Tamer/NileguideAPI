using Microsoft.EntityFrameworkCore;
using NileGuideApi.Data;
using NileGuideApi.DTOs;

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
                .Select(a => new ActivityCardDto
                {
                    ActivityID = a.ActivityID,

                    ActivityName = a.ActivityName ?? string.Empty,
                    Description = a.Description ?? string.Empty,

                    CategoryID = a.CategoryID,
                    CategoryName = a.Category != null
                        ? a.Category.CategoryName ?? string.Empty
                        : string.Empty,

                    CityID = a.CityID,
                    CityName = a.City != null
                        ? a.City.CityName ?? string.Empty
                        : string.Empty,

                    MinPrice = a.MinPrice ?? 0,
                    PriceCurrency = a.PriceCurrency ?? "USD",

                    ImageUrl = a.ActivityImages
                        .OrderByDescending(img => img.IsPrimary)
                        .ThenBy(img => img.SortOrder)
                        .Select(img => img.Url ?? string.Empty)
                        .FirstOrDefault() ?? string.Empty,

                    RequiredDocuments = a.RequiredDocuments ?? string.Empty,

                    IsActive = true,

                    Rating = a.Rating,
                    ReviewsCount = a.ReviewCount,

                    Providers = a.BookingLinks
                        .OrderBy(link => link.Id)
                        .Select(link => new ActivityProviderDto
                        {
                            ProviderName = link.Provider ?? string.Empty,
                            Link = link.Url ?? string.Empty
                        })
                        .ToList(),

                    OpeningHours = a.ActivityHours
                        .OrderBy(hour => hour.Id)
                        .Select(hour => new ActivityHourDto
                        {
                            Day = "Daily",
                            OpenTime = FormatHourWithPeriod(hour.OpeningHour, hour.OpeningPeriod),
                            CloseTime = FormatHourWithPeriod(hour.ClosingHour, hour.ClosingPeriod)
                        })
                        .ToList()
                })
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
                .FirstOrDefaultAsync(a => a.ActivityID == id);

            if (activity == null)
                return null;

            return new ActivityDetailsDto
            {
                ActivityID = activity.ActivityID,

                ActivityName = activity.ActivityName ?? string.Empty,
                Description = activity.Description ?? string.Empty,

                CategoryID = activity.CategoryID,
                CategoryName = activity.Category != null
                    ? activity.Category.CategoryName ?? string.Empty
                    : string.Empty,

                CityID = activity.CityID,
                CityName = activity.City != null
                    ? activity.City.CityName ?? string.Empty
                    : string.Empty,

                Latitude = activity.Latitude
                    ?? (activity.City?.Latitude != null ? Convert.ToDouble(activity.City.Latitude.Value) : 0),

                Longitude = activity.Longitude
                    ?? (activity.City?.Longitude != null ? Convert.ToDouble(activity.City.Longitude.Value) : 0),

                Price = activity.Price ?? 0,
                MinPrice = activity.MinPrice ?? 0,

                PriceCurrency = activity.PriceCurrency ?? "USD",
                PriceBasis = activity.PriceBasis ?? string.Empty,

                Duration = activity.Duration ?? string.Empty,
                GroupSize = activity.GroupSize ?? string.Empty,
                Cancellation = activity.Cancellation ?? string.Empty,
                RequiredDocuments = activity.RequiredDocuments ?? string.Empty,

                IsActive = true,

                Images = activity.ActivityImages
                    .OrderByDescending(img => img.IsPrimary)
                    .ThenBy(img => img.SortOrder)
                    .Select(img => img.Url ?? string.Empty)
                    .ToList(),

                Providers = activity.BookingLinks
                    .OrderBy(link => link.Id)
                    .Select(link => new ActivityProviderDto
                    {
                        ProviderName = link.Provider ?? string.Empty,
                        Link = link.Url ?? string.Empty
                    })
                    .ToList(),

                OpeningHours = activity.ActivityHours
                    .OrderBy(hour => hour.Id)
                    .Select(hour => new ActivityHourDto
                    {
                        Day = "Daily",
                        OpenTime = FormatHourWithPeriod(hour.OpeningHour, hour.OpeningPeriod),
                        CloseTime = FormatHourWithPeriod(hour.ClosingHour, hour.ClosingPeriod)
                    })
                    .ToList()
            };
        }

        private static string FormatHourWithPeriod(byte? hour, string? period)
        {
            if (!hour.HasValue)
                return string.Empty;

            var cleanPeriod = period ?? string.Empty;

            return $"{hour.Value}:00 {cleanPeriod}".Trim();
        }
    }
}
