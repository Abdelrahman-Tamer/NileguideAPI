using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NileGuideApi.Data;
using NileGuideApi.DTOs;
using NileGuideApi.Hubs;
using NileGuideApi.Models;

namespace NileGuideApi.Services
{
    public class AdminActivityService : IAdminActivityService
    {
        private readonly AppDbContext _context;
        private readonly IActivityImageService _activityImageService;
        private readonly IHubContext<DashboardHub> _hubContext;

        public AdminActivityService(
            AppDbContext context,
            IActivityImageService activityImageService,
            IHubContext<DashboardHub> hubContext)
        {
            _context = context;
            _activityImageService = activityImageService;
            _hubContext = hubContext;
        }

        public async Task<AdminActivityDetailsDto> CreateAsync(CreateActivityDto dto)
        {
            var bookingLinkDtos = dto.BookingLinks ?? new List<CreateBookingLinkDto>();
            var activityHourDtos = dto.ActivityHours ?? new List<CreateActivityHourDto>();

            var categoryExists = await _context.Categories
                .AnyAsync(x => x.CategoryID == dto.CategoryId && x.DeletedAt == null);

            if (!categoryExists)
                throw new KeyNotFoundException("Category not found");

            var cityExists = await _context.Cities
                .AnyAsync(x => x.CityID == dto.CityId && x.DeletedAt == null);

            if (!cityExists)
                throw new KeyNotFoundException("City not found");

            if (!string.IsNullOrWhiteSpace(dto.ExternalId))
            {
                var externalExists = await _context.Activities.AnyAsync(x =>
                    x.ExternalId == dto.ExternalId && x.DeletedAt == null);

                if (externalExists)
                    throw new InvalidOperationException("ExternalId already exists");
            }

            ValidateActivityInput(
                dto.ActivityName,
                dto.Description,
                dto.Duration,
                dto.GroupSize,
                dto.Price,
                dto.PriceCurrency);
            ValidateBookingLinks(bookingLinkDtos);
            ValidateActivityHours(activityHourDtos);

            if (dto.Images == null || dto.Images.Count == 0)
                throw new InvalidOperationException("At least one image is required");

            var activity = new Activity
            {
                ActivityName = dto.ActivityName.Trim(),
                Description = dto.Description.Trim(),
                CategoryID = dto.CategoryId,
                CityID = dto.CityId,
                Price = dto.Price,
                MinPrice = dto.MinPrice,
                PriceCurrency = string.IsNullOrWhiteSpace(dto.PriceCurrency) ? "USD" : dto.PriceCurrency.Trim(),
                PriceBasis = string.IsNullOrWhiteSpace(dto.PriceBasis) ? null : dto.PriceBasis.Trim(),
                Duration = dto.Duration,
                GroupSize = dto.GroupSize.Trim(),
                Cancellation = string.IsNullOrWhiteSpace(dto.Cancellation) ? null : dto.Cancellation.Trim(),
                RequiredDocuments = string.IsNullOrWhiteSpace(dto.RequiredDocuments) ? null : dto.RequiredDocuments.Trim(),
                Provider = string.IsNullOrWhiteSpace(dto.Provider) ? null : dto.Provider.Trim(),
                ExternalId = string.IsNullOrWhiteSpace(dto.ExternalId) ? null : dto.ExternalId.Trim(),
                Region = string.IsNullOrWhiteSpace(dto.Region) ? null : dto.Region.Trim(),
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Rating = dto.Rating,
                ReviewCount = 0,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            var urls = await _activityImageService.UploadAsync(dto.Images, activity.ActivityID);

            var images = urls.Select((url, index) => new ActivityImage
            {
                ActivityID = activity.ActivityID,
                Url = url,
                IsPrimary = index == 0,
                SortOrder = index + 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList();

            _context.ActivityImages.AddRange(images);

            if (bookingLinkDtos.Count > 0)
            {
                var bookingLinks = bookingLinkDtos.Select(link => new BookingLink
                {
                    ActivityID = activity.ActivityID,
                    Provider = link.Provider.Trim(),
                    Url = link.Url.Trim()
                });

                _context.BookingLinks.AddRange(bookingLinks);
            }

            if (activityHourDtos.Count > 0)
            {
                var activityHours = activityHourDtos.Select(hour => new ActivityHour
                {
                    ActivityID = activity.ActivityID,
                    OpenHour = hour.OpenHour,
                    OpenAmPm = NormalizePeriod(hour.OpenAmPm),
                    CloseHour = hour.CloseHour,
                    CloseAmPm = NormalizePeriod(hour.CloseAmPm)
                });

                _context.ActivityHours.AddRange(activityHours);
            }

            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ActivitiesUpdated");

            var createdActivity = await _context.Activities
                .AsNoTracking()
                .Include(x => x.ActivityImages)
                .Include(x => x.BookingLinks)
                .Include(x => x.ActivityHours)
                .FirstAsync(x => x.ActivityID == activity.ActivityID);

            return MapToDetailsDto(createdActivity);
        }

        public async Task<AdminActivityDetailsDto> UpdateAsync(int id, UpdateActivityDto dto)
        {
            var activity = await _context.Activities
                .Include(x => x.ActivityImages)
                .Include(x => x.BookingLinks)
                .Include(x => x.ActivityHours)
                .FirstOrDefaultAsync(x => x.ActivityID == id && x.DeletedAt == null);

            if (activity == null)
                throw new KeyNotFoundException("Activity not found");

            var categoryExists = await _context.Categories
                .AnyAsync(x => x.CategoryID == dto.CategoryId && x.DeletedAt == null);

            if (!categoryExists)
                throw new KeyNotFoundException("Category not found");

            var cityExists = await _context.Cities
                .AnyAsync(x => x.CityID == dto.CityId && x.DeletedAt == null);

            if (!cityExists)
                throw new KeyNotFoundException("City not found");

            if (!string.IsNullOrWhiteSpace(dto.ExternalId))
            {
                var externalExists = await _context.Activities.AnyAsync(x =>
                    x.ActivityID != id &&
                    x.ExternalId == dto.ExternalId &&
                    x.DeletedAt == null);

                if (externalExists)
                    throw new InvalidOperationException("ExternalId already exists");
            }

            ValidateActivityInput(
                dto.ActivityName,
                dto.Description,
                dto.Duration,
                dto.GroupSize,
                dto.Price,
                dto.PriceCurrency);
            if (dto.BookingLinks != null)
                ValidateBookingLinks(dto.BookingLinks);

            if (dto.ActivityHours != null)
                ValidateActivityHours(dto.ActivityHours);

            activity.ActivityName = dto.ActivityName.Trim();
            activity.Description = dto.Description.Trim();
            activity.CategoryID = dto.CategoryId;
            activity.CityID = dto.CityId;
            activity.Price = dto.Price;
            activity.MinPrice = dto.MinPrice;
            activity.PriceCurrency = string.IsNullOrWhiteSpace(dto.PriceCurrency) ? "USD" : dto.PriceCurrency.Trim();
            activity.PriceBasis = string.IsNullOrWhiteSpace(dto.PriceBasis) ? null : dto.PriceBasis.Trim();
            activity.Duration = dto.Duration;
            activity.GroupSize = dto.GroupSize.Trim();
            activity.Cancellation = string.IsNullOrWhiteSpace(dto.Cancellation) ? null : dto.Cancellation.Trim();
            activity.RequiredDocuments = string.IsNullOrWhiteSpace(dto.RequiredDocuments) ? null : dto.RequiredDocuments.Trim();
            activity.Provider = string.IsNullOrWhiteSpace(dto.Provider) ? null : dto.Provider.Trim();
            activity.ExternalId = string.IsNullOrWhiteSpace(dto.ExternalId) ? null : dto.ExternalId.Trim();
            activity.Region = string.IsNullOrWhiteSpace(dto.Region) ? null : dto.Region.Trim();
            activity.Latitude = dto.Latitude;
            activity.Longitude = dto.Longitude;
            activity.Rating = dto.Rating;
            activity.IsActive = dto.IsActive;
            activity.UpdatedAt = DateTime.UtcNow;

            if (dto.ReplaceImages && dto.Images is { Count: > 0 })
            {
                var existingUrls = activity.ActivityImages
                    .Where(x => x.DeletedAt == null)
                    .Select(x => x.Url)
                    .ToList();

                await _activityImageService.DeleteManyByUrlsAsync(existingUrls, id);

                foreach (var oldImage in activity.ActivityImages.Where(x => x.DeletedAt == null))
                {
                    oldImage.DeletedAt = DateTime.UtcNow;
                    oldImage.UpdatedAt = DateTime.UtcNow;
                }

                var urls = await _activityImageService.UploadAsync(dto.Images, id);

                var newImages = urls.Select((url, index) => new ActivityImage
                {
                    ActivityID = id,
                    Url = url,
                    IsPrimary = index == 0,
                    SortOrder = index + 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList();

                _context.ActivityImages.AddRange(newImages);
            }

            if (dto.BookingLinks != null)
            {
                _context.BookingLinks.RemoveRange(activity.BookingLinks);

                if (dto.BookingLinks.Count > 0)
                {
                    var bookingLinks = dto.BookingLinks.Select(link => new BookingLink
                    {
                        ActivityID = id,
                        Provider = link.Provider.Trim(),
                        Url = link.Url.Trim()
                    });

                    _context.BookingLinks.AddRange(bookingLinks);
                }
            }

            if (dto.ActivityHours != null)
            {
                _context.ActivityHours.RemoveRange(activity.ActivityHours);

                if (dto.ActivityHours.Count > 0)
                {
                    var activityHours = dto.ActivityHours.Select(hour => new ActivityHour
                    {
                        ActivityID = id,
                        OpenHour = hour.OpenHour,
                        OpenAmPm = NormalizePeriod(hour.OpenAmPm),
                        CloseHour = hour.CloseHour,
                        CloseAmPm = NormalizePeriod(hour.CloseAmPm)
                    });

                    _context.ActivityHours.AddRange(activityHours);
                }
            }

            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("ActivitiesUpdated");

            var updatedActivity = await _context.Activities
                .AsNoTracking()
                .Include(x => x.ActivityImages)
                .Include(x => x.BookingLinks)
                .Include(x => x.ActivityHours)
                .FirstAsync(x => x.ActivityID == id && x.DeletedAt == null);

            return MapToDetailsDto(updatedActivity);
        }

        public async Task DeleteAsync(int id)
        {
            var activity = await _context.Activities
                .Include(x => x.ActivityImages)
                .FirstOrDefaultAsync(x => x.ActivityID == id && x.DeletedAt == null);

            if (activity == null)
                throw new KeyNotFoundException("Activity not found");

            var existingUrls = activity.ActivityImages
                .Where(x => x.DeletedAt == null)
                .Select(x => x.Url)
                .ToList();

            await _activityImageService.DeleteManyByUrlsAsync(existingUrls, id);

            foreach (var image in activity.ActivityImages.Where(x => x.DeletedAt == null))
            {
                image.DeletedAt = DateTime.UtcNow;
                image.UpdatedAt = DateTime.UtcNow;
            }

            activity.DeletedAt = DateTime.UtcNow;
            activity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("ActivitiesUpdated");
        }

        private static void ValidateActivityInput(
            string activityName,
            string description,
            int duration,
            string groupSize,
            decimal price,
            string priceCurrency)
        {
            if (string.IsNullOrWhiteSpace(activityName))
                throw new InvalidOperationException("Activity name is required");

            if (string.IsNullOrWhiteSpace(description))
                throw new InvalidOperationException("Description is required");

            if (duration <= 0)
                throw new InvalidOperationException("Duration must be greater than 0");

            if (string.IsNullOrWhiteSpace(groupSize))
                throw new InvalidOperationException("Group size is required");

            if (price <= 0)
                throw new InvalidOperationException("Price must be greater than 0");

            if (string.IsNullOrWhiteSpace(priceCurrency))
                throw new InvalidOperationException("Price currency is required");
        }

        private static void ValidateBookingLinks(IReadOnlyCollection<CreateBookingLinkDto> bookingLinks)
        {
            foreach (var bookingLink in bookingLinks)
            {
                if (string.IsNullOrWhiteSpace(bookingLink.Provider))
                    throw new InvalidOperationException("Booking link provider is required");

                if (bookingLink.Provider.Trim().Length > 50)
                    throw new InvalidOperationException("Booking link provider must be 50 characters or fewer");

                if (string.IsNullOrWhiteSpace(bookingLink.Url))
                    throw new InvalidOperationException("Booking link url is required");
            }
        }

        private static void ValidateActivityHours(IReadOnlyCollection<CreateActivityHourDto> activityHours)
        {
            foreach (var activityHour in activityHours)
            {
                if (activityHour.OpenHour is < 1 or > 12)
                    throw new InvalidOperationException("Activity open hour must be between 1 and 12");

                if (activityHour.CloseHour is < 1 or > 12)
                    throw new InvalidOperationException("Activity close hour must be between 1 and 12");

                if (!IsValidPeriod(activityHour.OpenAmPm))
                    throw new InvalidOperationException("Activity open period must be AM or PM");

                if (!IsValidPeriod(activityHour.CloseAmPm))
                    throw new InvalidOperationException("Activity close period must be AM or PM");
            }
        }

        private static AdminActivityDetailsDto MapToDetailsDto(Activity activity)
        {
            return new AdminActivityDetailsDto
            {
                ActivityId = activity.ActivityID,
                ActivityName = activity.ActivityName,
                Description = activity.Description ?? string.Empty,
                CategoryId = activity.CategoryID,
                CityId = activity.CityID,
                Price = activity.Price ?? 0,
                MinPrice = activity.MinPrice,
                PriceCurrency = activity.PriceCurrency,
                PriceBasis = activity.PriceBasis,
                Duration = activity.Duration,
                GroupSize = activity.GroupSize ?? string.Empty,
                Cancellation = activity.Cancellation,
                RequiredDocuments = activity.RequiredDocuments,
                Provider = activity.Provider,
                ExternalId = activity.ExternalId,
                Region = activity.Region,
                Latitude = activity.Latitude,
                Longitude = activity.Longitude,
                Rating = activity.Rating,
                ReviewCount = activity.ReviewCount,
                IsActive = activity.IsActive,
                Images = activity.ActivityImages
                    .Where(x => x.DeletedAt == null)
                    .OrderByDescending(x => x.IsPrimary)
                    .ThenBy(x => x.SortOrder)
                    .Select(x => new ActivityImageDto
                    {
                        ImageId = x.ImageID,
                        Url = x.Url,
                        IsPrimary = x.IsPrimary,
                        SortOrder = x.SortOrder
                    })
                    .ToList(),
                BookingLinks = activity.BookingLinks
                    .OrderBy(x => x.Id)
                    .Select(x => new AdminBookingLinkDto
                    {
                        BookingLinkId = x.Id,
                        Provider = x.Provider ?? string.Empty,
                        Url = x.Url ?? string.Empty
                    })
                    .ToList(),
                ActivityHours = activity.ActivityHours
                    .OrderBy(x => x.Id)
                    .Select(x => new AdminActivityHourDto
                    {
                        ActivityHourId = x.Id,
                        OpenHour = x.OpenHour,
                        OpenAmPm = NormalizePeriod(x.OpenAmPm),
                        CloseHour = x.CloseHour,
                        CloseAmPm = NormalizePeriod(x.CloseAmPm),
                        OpenTime = FormatHourWithPeriod(x.OpenHour, x.OpenAmPm),
                        CloseTime = FormatHourWithPeriod(x.CloseHour, x.CloseAmPm)
                    })
                    .ToList()
            };
        }

        private static string FormatHourWithPeriod(byte hour, string? period)
        {
            return $"{hour}:00 {NormalizePeriod(period)}".Trim();
        }

        private static string NormalizePeriod(string? period)
        {
            return (period ?? string.Empty).Trim().ToUpperInvariant();
        }

        private static bool IsValidPeriod(string? period)
        {
            var normalizedPeriod = NormalizePeriod(period);
            return normalizedPeriod is "AM" or "PM";
        }
    }
}
