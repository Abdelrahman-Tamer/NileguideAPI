using System.ComponentModel.DataAnnotations;

namespace NileGuideApi.DTOs
{
    public class CreateReviewDto
    {
        [Required]
        [MaxLength(100)]
        public string? ReviewerCity { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Comment { get; set; } = string.Empty;
    }

    public class ReviewDto
    {
        public int ReviewId { get; set; }
        public int ActivityId { get; set; }
        public int UserId { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public string? ReviewerCity { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}