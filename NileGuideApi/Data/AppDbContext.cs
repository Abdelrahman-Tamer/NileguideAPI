using Microsoft.EntityFrameworkCore;
using NileGuideApi.Models;

namespace NileGuideApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<PasswordResetToken>()
                .HasIndex(p => p.UserId);

            modelBuilder.Entity<PasswordResetToken>()
                .HasIndex(p => p.TokenHash);

            // Soft delete filter for User
            modelBuilder.Entity<User>().HasQueryFilter(u => u.DeletedAt == null);

            // Configure relationship as optional to fix warning
            modelBuilder.Entity<PasswordResetToken>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Optional: Filter for PasswordResetToken based on User's DeletedAt
            modelBuilder.Entity<PasswordResetToken>().HasQueryFilter(p => p.User!.DeletedAt == null);
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries<User>().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);
            foreach (var entry in entries)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}