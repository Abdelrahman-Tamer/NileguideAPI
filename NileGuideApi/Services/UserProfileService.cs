using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NileGuideApi.Data;
using NileGuideApi.DTOs;
using NileGuideApi.Models;

namespace NileGuideApi.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly AppDbContext _context;

        public UserProfileService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserProfileResponseDto?> GetMyProfileAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return null;

            if (user.Profile == null)
            {
                user.Profile = CreateDefaultProfile(user.Id);
                _context.UserProfiles.Add(user.Profile);
                await _context.SaveChangesAsync();
            }

            return await BuildProfileResponseAsync(user);
        }

        public async Task<UserProfileResponseDto?> UpdateMyProfileAsync(int userId, UpdateUserProfileDto dto)
        {
            var user = await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return null;

            var cleanCityIds = NormalizeIds(dto.PreferredCityIds);
            var cleanCategoryIds = NormalizeIds(dto.InterestCategoryIds);

            var validCityCount = await _context.Cities
                .CountAsync(c => cleanCityIds.Contains(c.CityID));

            if (validCityCount != cleanCityIds.Count)
                throw new InvalidOperationException("Some selected cities do not exist");

            var validCategoryCount = await _context.Categories
                .CountAsync(c => cleanCategoryIds.Contains(c.CategoryID));

            if (validCategoryCount != cleanCategoryIds.Count)
                throw new InvalidOperationException("Some selected categories do not exist");

            var now = DateTime.UtcNow;

            if (user.Profile == null)
            {
                user.Profile = CreateDefaultProfile(user.Id);
                _context.UserProfiles.Add(user.Profile);
            }

            // Updates basic user data only when the frontend sends it.
            if (!string.IsNullOrWhiteSpace(dto.FullName))
            {
                user.FullName = dto.FullName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var normalizedEmail = dto.Email.Trim().ToLower();

                var emailExists = await _context.Users
                    .AnyAsync(u => u.Id != userId && u.Email.ToLower() == normalizedEmail);

                if (emailExists)
                    throw new InvalidOperationException("Email is already used by another user");

                user.Email = normalizedEmail;
            }

            if (dto.DateOfBirth.HasValue)
            {
                user.DateOfBirth = dto.DateOfBirth.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.Nationality))
            {
                user.Nationality = dto.Nationality.Trim();
            }

            user.UpdatedAt = now;

            // Required profile fields stay required and keep the old behavior.
            user.Profile.HasTravelDates = dto.HasTravelDates;

            if (dto.HasTravelDates)
            {
                user.Profile.TravelStartDate = dto.TravelStartDate;
                user.Profile.TravelEndDate = dto.TravelEndDate;
            }
            else
            {
                user.Profile.TravelStartDate = DateOnly.MinValue;
                user.Profile.TravelEndDate = DateOnly.MinValue;
            }

            user.Profile.BudgetLevel = dto.BudgetLevel.Trim();
            user.Profile.PreferredCityIdsJson = JsonSerializer.Serialize(cleanCityIds);
            user.Profile.InterestCategoryIdsJson = JsonSerializer.Serialize(cleanCategoryIds);
            user.Profile.UpdatedAt = now;

            await _context.SaveChangesAsync();

            return await BuildProfileResponseAsync(user);
        }

        private static UserProfile CreateDefaultProfile(int userId)
        {
            var now = DateTime.UtcNow;

            return new UserProfile
            {
                UserId = userId,
                HasTravelDates = false,
                TravelStartDate = DateOnly.MinValue,
                TravelEndDate = DateOnly.MinValue,
                BudgetLevel = string.Empty,
                PreferredCityIdsJson = "[]",
                InterestCategoryIdsJson = "[]",
                CreatedAt = now,
                UpdatedAt = now
            };
        }

        private async Task<UserProfileResponseDto> BuildProfileResponseAsync(User user)
        {
            var profile = user.Profile ?? CreateDefaultProfile(user.Id);

            var preferredCityIds = DeserializeIds(profile.PreferredCityIdsJson);
            var interestCategoryIds = DeserializeIds(profile.InterestCategoryIdsJson);

            var preferredCities = await _context.Cities
                .Where(c => preferredCityIds.Contains(c.CityID))
                .Select(c => new SelectedLookupDto
                {
                    Id = c.CityID,
                    Name = c.CityName
                })
                .ToListAsync();

            var interests = await _context.Categories
                .Where(c => interestCategoryIds.Contains(c.CategoryID))
                .Select(c => new SelectedLookupDto
                {
                    Id = c.CategoryID,
                    Name = c.CategoryName
                })
                .ToListAsync();

            return new UserProfileResponseDto
            {
                UserId = user.Id,

                Email = user.Email,
                FullName = user.FullName,
                Nationality = user.Nationality,
                DateOfBirth = user.DateOfBirth,
                Age = CalculateAge(user.DateOfBirth),

                HasTravelDates = profile.HasTravelDates,
                TravelStartDate = profile.TravelStartDate,
                TravelEndDate = profile.TravelEndDate,
                BudgetLevel = profile.BudgetLevel,

                PreferredCityIds = preferredCityIds,
                PreferredCities = preferredCities,

                InterestCategoryIds = interestCategoryIds,
                Interests = interests,

                Role = user.Role
            };
        }

        private static int CalculateAge(DateOnly dateOfBirth)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var age = today.Year - dateOfBirth.Year;

            if (dateOfBirth > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }

        private static List<int> NormalizeIds(List<int>? ids)
        {
            return ids?
                .Where(id => id > 0)
                .Distinct()
                .ToList() ?? new List<int>();
        }

        private static List<int> DeserializeIds(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<int>();

            try
            {
                return JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
            }
            catch
            {
                return new List<int>();
            }
        }
    }
}