using Microsoft.EntityFrameworkCore;
using NileGuideApi.Data;
using NileGuideApi.DTOs;
using NileGuideApi.Models;

namespace NileGuideApi.Services
{
    public class ReviewService : IReviewService
    {
        private readonly AppDbContext _context;

        public ReviewService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ReviewDto>> GetAllByActivityIdAsync(int activityId)
        {
            var activityExists = await _context.Activities
                .AnyAsync(x => x.ActivityID == activityId && x.DeletedAt == null);

            if (!activityExists)
                throw new KeyNotFoundException("Activity not found");

            var reviews = await _context.Reviews
                .AsNoTracking()
                .Where(x => x.ActivityId == activityId && x.DeletedAt == null)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new ReviewDto
                {
                    ReviewId = x.ReviewId,
                    ActivityId = x.ActivityId,
                    UserId = x.UserId,
                    ReviewerName = x.ReviewerName,
                    ReviewerCity = x.ReviewerCity,
                    Rating = x.Rating,
                    Comment = x.Comment,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            return reviews;
        }

        public async Task<ReviewDto> CreateAsync(int activityId, int userId, CreateReviewDto dto)
        {
            var activity = await _context.Activities
                .FirstOrDefaultAsync(x => x.ActivityID == activityId && x.DeletedAt == null);

            if (activity == null)
                throw new KeyNotFoundException("Activity not found");

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == userId && x.DeletedAt == null);

            if (user == null)
                throw new UnauthorizedAccessException("User not found");

            if (!user.IsActive)
                throw new InvalidOperationException("Your account is blocked");

            var review = new Review
            {
                ActivityId = activityId,
                UserId = user.Id,
                ReviewerName = user.FullName,
                ReviewerCity = string.IsNullOrWhiteSpace(dto.ReviewerCity) ? null : dto.ReviewerCity.Trim(),
                Rating = dto.Rating,
                Comment = dto.Comment.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            var activeReviews = await _context.Reviews
                .Where(x => x.ActivityId == activityId && x.DeletedAt == null)
                .ToListAsync();

            activity.ReviewCount = activeReviews.Count;
            activity.Rating = activeReviews.Count == 0
                ? 0
                : (float)activeReviews.Average(x => x.Rating);

            activity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ReviewDto
            {
                ReviewId = review.ReviewId,
                ActivityId = review.ActivityId,
                UserId = review.UserId,
                ReviewerName = review.ReviewerName,
                ReviewerCity = review.ReviewerCity,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };
        }
    }
}