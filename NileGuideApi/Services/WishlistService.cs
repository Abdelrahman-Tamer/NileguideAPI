using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NileGuideApi.Data;
using NileGuideApi.DTOs;
using NileGuideApi.Models;

namespace NileGuideApi.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly AppDbContext _context;

        public WishlistService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResultDto<ActivityCardDto>> GetWishlistAsync(int userId, WishlistFilterDto filter)
        {
            if (filter.Page <= 0)
                filter.Page = 1;

            if (filter.PageSize <= 0)
                filter.PageSize = 9;

            var query = _context.WishlistItems
                .AsNoTracking()
                .Where(item => item.UserId == userId && item.Activity != null && item.Activity.IsActive)
                .Include(item => item.Activity)
                    .ThenInclude(activity => activity!.Category)
                .Include(item => item.Activity)
                    .ThenInclude(activity => activity!.City)
                .Include(item => item.Activity)
                    .ThenInclude(activity => activity!.ActivityImages)
                .Include(item => item.Activity)
                    .ThenInclude(activity => activity!.BookingLinks)
                .Include(item => item.Activity)
                    .ThenInclude(activity => activity!.ActivityHours)
                .OrderByDescending(item => item.CreatedAtUtc)
                .ThenByDescending(item => item.Id)
                .AsSplitQuery();

            var totalCount = await query.CountAsync();

            var wishlistItems = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResultDto<ActivityCardDto>
            {
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                Items = wishlistItems
                    .Where(item => item.Activity != null)
                    .Select(item => ActivityDtoMapper.ToCardDto(item.Activity!))
                    .ToList()
            };
        }

        public async Task<List<int>> GetActivityIdsAsync(int userId)
        {
            return await _context.WishlistItems
                .AsNoTracking()
                .Where(item => item.UserId == userId && item.Activity != null && item.Activity.IsActive)
                .OrderByDescending(item => item.CreatedAtUtc)
                .ThenByDescending(item => item.Id)
                .Select(item => item.ActivityID)
                .ToListAsync();
        }

        public async Task<WishlistAddResult> AddAsync(int userId, int activityId)
        {
            var activityExists = await _context.Activities
                .AsNoTracking()
                .AnyAsync(activity => activity.ActivityID == activityId && activity.IsActive);

            if (!activityExists)
                return WishlistAddResult.ActivityNotFound;

            var alreadyExists = await _context.WishlistItems
                .AsNoTracking()
                .AnyAsync(item => item.UserId == userId && item.ActivityID == activityId);

            if (alreadyExists)
                return WishlistAddResult.AlreadyExists;

            var wishlistItem = new WishlistItem
            {
                UserId = userId,
                ActivityID = activityId,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.WishlistItems.Add(wishlistItem);

            try
            {
                await _context.SaveChangesAsync();
                return WishlistAddResult.Added;
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                _context.Entry(wishlistItem).State = EntityState.Detached;
                return WishlistAddResult.AlreadyExists;
            }
        }

        public async Task RemoveAsync(int userId, int activityId)
        {
            var wishlistItem = await _context.WishlistItems
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(item => item.UserId == userId && item.ActivityID == activityId);

            if (wishlistItem == null)
                return;

            _context.WishlistItems.Remove(wishlistItem);
            await _context.SaveChangesAsync();
        }

        public async Task<WishlistStatusDto?> GetStatusAsync(int userId, int activityId)
        {
            var activityExists = await _context.Activities
                .AsNoTracking()
                .AnyAsync(activity => activity.ActivityID == activityId && activity.IsActive);

            if (!activityExists)
                return null;

            var isWishlisted = await _context.WishlistItems
                .AsNoTracking()
                .AnyAsync(item => item.UserId == userId && item.ActivityID == activityId);

            return new WishlistStatusDto
            {
                ActivityID = activityId,
                IsWishlisted = isWishlisted
            };
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            return ex.InnerException is SqlException { Number: 2601 or 2627 };
        }
    }
}
