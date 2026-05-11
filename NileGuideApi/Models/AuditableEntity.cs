using System;

namespace NileGuideApi.Models
{
    // Shared audit fields for entities that support activation and soft delete.
    public abstract class AuditableEntity
    {
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
