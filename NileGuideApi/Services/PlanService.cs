using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NileGuideApi.Data;
using NileGuideApi.DTOs;
using NileGuideApi.Models;
using System.Globalization;

namespace NileGuideApi.Services
{
    public class PlanService : IPlanService
    {
        private readonly AppDbContext _context;

        public PlanService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PlanResponse> GetPlanAsync(int userId)
        {
            var planItems = await GetPlanItemsQuery(userId)
                .OrderBy(item => item.ScheduledDate)
                .ThenBy(item => item.StartTime)
                .ThenBy(item => item.Id)
                .ToListAsync();

            var items = planItems
                .Where(item => item.Activity != null)
                .Select(MapToResponse)
                .ToList();

            return new PlanResponse
            {
                TotalActivities = items.Count,
                TotalCost = items.Sum(item => item.Price),
                PriceCurrency = items.FirstOrDefault()?.PriceCurrency ?? "USD",
                Items = items
            };
        }

        public async Task<PlanItemResponse> AddItemAsync(int userId, AddPlanItemRequest request)
        {
            if (request.ActivityId <= 0)
                throw new ArgumentException("Activity id must be positive", nameof(request));

            if (request.ScheduledDate == default)
                throw new ArgumentException("ScheduledDate is required", nameof(request));

            if (!TimeOnly.TryParseExact(
                    request.StartTime,
                    "HH:mm",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var startTime))
            {
                throw new ArgumentException("StartTime must be in HH:mm format", nameof(request));
            }

            var existingItem = await GetPlanItemsQuery(userId)
                .FirstOrDefaultAsync(item => item.ActivityId == request.ActivityId);

            if (existingItem != null)
                return MapToResponse(existingItem);

            var activityExists = await _context.Activities
                .AsNoTracking()
                .AnyAsync(activity => activity.ActivityID == request.ActivityId && activity.IsActive);

            if (!activityExists)
                throw new KeyNotFoundException("Activity not found");

            var planItem = new PlanItem
            {
                UserId = userId,
                ActivityId = request.ActivityId,
                ScheduledDate = request.ScheduledDate,
                StartTime = startTime,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _context.PlanItems.Add(planItem);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                _context.Entry(planItem).State = EntityState.Detached;

                var concurrentItem = await GetPlanItemsQuery(userId)
                    .FirstAsync(item => item.ActivityId == request.ActivityId);

                return MapToResponse(concurrentItem);
            }

            var savedItem = await GetPlanItemsQuery(userId)
                .FirstAsync(item => item.Id == planItem.Id);

            return MapToResponse(savedItem);
        }

        public async Task DeleteItemAsync(int userId, int planItemId)
        {
            var planItem = await _context.PlanItems
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(item => item.UserId == userId && item.Id == planItemId);

            if (planItem == null)
                throw new KeyNotFoundException("Plan item not found");

            _context.PlanItems.Remove(planItem);
            await _context.SaveChangesAsync();
        }

        public async Task<List<int>> GetActivityIdsAsync(int userId)
        {
            return await _context.PlanItems
                .AsNoTracking()
                .Where(item => item.UserId == userId)
                .OrderBy(item => item.ScheduledDate)
                .ThenBy(item => item.StartTime)
                .ThenBy(item => item.Id)
                .Select(item => item.ActivityId)
                .ToListAsync();
        }

        private IQueryable<PlanItem> GetPlanItemsQuery(int userId)
        {
            return _context.PlanItems
                .AsNoTracking()
                .Where(item => item.UserId == userId)
                .Include(item => item.Activity)
                    .ThenInclude(activity => activity!.City)
                .Include(item => item.Activity)
                    .ThenInclude(activity => activity!.BookingLinks)
                .AsSplitQuery();
        }

        private static PlanItemResponse MapToResponse(PlanItem item)
        {
            var activity = item.Activity
                ?? throw new InvalidOperationException("Plan item activity was not loaded");

            var endTime = item.StartTime.AddMinutes(activity.Duration);

            return new PlanItemResponse
            {
                PlanItemId = item.Id,
                ActivityId = item.ActivityId,
                ScheduledDate = item.ScheduledDate,
                StartTime = item.StartTime.ToString("HH:mm", CultureInfo.InvariantCulture),
                EndTime = endTime.ToString("HH:mm", CultureInfo.InvariantCulture),
                ActivityName = activity.ActivityName ?? string.Empty,
                Description = activity.Description ?? string.Empty,
                CityId = activity.CityID,
                CityName = activity.City?.CityName ?? string.Empty,
                DurationMinutes = activity.Duration,
                Price = activity.Price ?? activity.MinPrice ?? 0,
                PriceCurrency = activity.PriceCurrency ?? "USD",
                IsActive = activity.IsActive,
                BookingLinks = activity.BookingLinks
                    .OrderBy(link => link.Id)
                    .Select(link => new BookingLinkDto
                    {
                        ProviderName = link.Provider ?? string.Empty,
                        Link = link.Url ?? string.Empty
                    })
                    .ToList()
            };
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            return ex.InnerException is SqlException { Number: 2601 or 2627 };
        }
    }
}
