using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace NileGuideApi.DTOs
{
    public class AdminActivityDetailsDto
    {
        public int ActivityId { get; set; }
        public string ActivityName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public int CityId { get; set; }
        public decimal Price { get; set; }
        public decimal? MinPrice { get; set; }
        public string PriceCurrency { get; set; } = "USD";
        public string? PriceBasis { get; set; }
        public int Duration { get; set; }
        public string GroupSize { get; set; } = string.Empty;
        public string? Cancellation { get; set; }
        public string? RequiredDocuments { get; set; }
        public string? Provider { get; set; }
        public string? ExternalId { get; set; }
        public string? Region { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsActive { get; set; }
        public List<ActivityImageDto> Images { get; set; } = new();
    }

    public class ActivityImageDto
    {
        public int ImageId { get; set; }
        public string Url { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
    }

    public class CreateActivityDto
    {
        [Required]
        [MinLength(3)]
        [MaxLength(255)]
        public string ActivityName { get; set; } = string.Empty;

        [Required]
        [MinLength(10)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int CityId { get; set; }

        [Required]
        [Range(typeof(decimal), "0.01", "999999999")]
        public decimal Price { get; set; }

        public decimal? MinPrice { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(10)]
        public string PriceCurrency { get; set; } = "USD";

        [MaxLength(50)]
        public string? PriceBasis { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Duration { get; set; }

        [Required]
        [MinLength(1)]
        [MaxLength(100)]
        public string GroupSize { get; set; } = string.Empty;

        public string? Cancellation { get; set; }
        public string? RequiredDocuments { get; set; }

        [MaxLength(50)]
        public string? Provider { get; set; }

        [MaxLength(100)]
        public string? ExternalId { get; set; }

        [MaxLength(100)]
        public string? Region { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [Range(0, 5)]
        public double Rating { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        [Required]
        public List<IFormFile> Images { get; set; } = new();
    }

    public class UpdateActivityDto
    {
        [Required]
        [MinLength(3)]
        [MaxLength(255)]
        public string ActivityName { get; set; } = string.Empty;

        [Required]
        [MinLength(10)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int CityId { get; set; }

        [Required]
        [Range(typeof(decimal), "0.01", "999999999")]
        public decimal Price { get; set; }

        public decimal? MinPrice { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(10)]
        public string PriceCurrency { get; set; } = "USD";

        [MaxLength(50)]
        public string? PriceBasis { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Duration { get; set; }

        [Required]
        [MinLength(1)]
        [MaxLength(100)]
        public string GroupSize { get; set; } = string.Empty;

        public string? Cancellation { get; set; }
        public string? RequiredDocuments { get; set; }

        [MaxLength(50)]
        public string? Provider { get; set; }

        [MaxLength(100)]
        public string? ExternalId { get; set; }

        [MaxLength(100)]
        public string? Region { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [Range(0, 5)]
        public double Rating { get; set; }

        public bool IsActive { get; set; }

        public bool ReplaceImages { get; set; } = false;

        public List<IFormFile>? Images { get; set; }
    }
}