using NileGuideApi.DTOs;
using NileGuideApi.Models;

namespace NileGuideApi.Services
{
    internal static class ActivityDtoMapper
    {
        public static ActivityCardDto ToCardDto(Activity activity)
        {
            return new ActivityCardDto
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

                MinPrice = activity.MinPrice ?? 0,
                PriceCurrency = activity.PriceCurrency ?? "USD",

                ImageUrl = activity.ActivityImages
                    .OrderByDescending(img => img.IsPrimary)
                    .ThenBy(img => img.SortOrder)
                    .Select(img => img.Url ?? string.Empty)
                    .FirstOrDefault() ?? string.Empty,

                RequiredDocuments = activity.RequiredDocuments ?? string.Empty,

                IsActive = activity.IsActive,

                Rating = activity.Rating,
                ReviewsCount = activity.ReviewCount,

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
                        OpenHour = hour.OpenHour,
                        OpenAmPm = NormalizePeriod(hour.OpenAmPm),
                        CloseHour = hour.CloseHour,
                        CloseAmPm = NormalizePeriod(hour.CloseAmPm),
                        OpenTime = FormatHourWithPeriod(hour.OpenHour, hour.OpenAmPm),
                        CloseTime = FormatHourWithPeriod(hour.CloseHour, hour.CloseAmPm)
                    })
                    .ToList()
            };
        }

        public static ActivityDetailsDto ToDetailsDto(Activity activity)
        {
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

                Duration = activity.Duration,
                GroupSize = activity.GroupSize ?? string.Empty,
                Cancellation = activity.Cancellation ?? string.Empty,
                RequiredDocuments = activity.RequiredDocuments ?? string.Empty,

                IsActive = activity.IsActive,

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
                        OpenHour = hour.OpenHour,
                        OpenAmPm = NormalizePeriod(hour.OpenAmPm),
                        CloseHour = hour.CloseHour,
                        CloseAmPm = NormalizePeriod(hour.CloseAmPm),
                        OpenTime = FormatHourWithPeriod(hour.OpenHour, hour.OpenAmPm),
                        CloseTime = FormatHourWithPeriod(hour.CloseHour, hour.CloseAmPm)
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
    }
}
