using Microsoft.EntityFrameworkCore;
using NileGuideApi.Models;

namespace NileGuideApi.Data
{
    // Central EF Core context for the API domain models.
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Registered application users.
        public DbSet<User> Users { get; set; } = null!;

        // Password reset attempts and issued reset codes.
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;

        // Rotating refresh tokens back the session lifecycle.
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        // Newsletter subscriptions are stored independently from user accounts.
        public DbSet<NewsletterSubscriber> NewsletterSubscribers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Soft-deleted users should disappear from normal queries.
            modelBuilder.Entity<User>()
                .HasQueryFilter(u => u.DeletedAt == null);

            // Email is a natural unique identifier for login and registration.
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Reset-code lookups are hash-based, so the hash column needs an index.
            modelBuilder.Entity<PasswordResetToken>()
                .HasIndex(x => x.TokenHash);

            // Reset tokens belong to a single user and are deleted with that user record.
            modelBuilder.Entity<PasswordResetToken>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Mirror the user soft-delete filter to avoid inconsistent token queries.
            modelBuilder.Entity<PasswordResetToken>()
                .HasQueryFilter(x => x.User != null && x.User.DeletedAt == null);

            modelBuilder.Entity<RefreshToken>()
                .ToTable("RefreshTokens");

            modelBuilder.Entity<RefreshToken>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<RefreshToken>()
                .Property(x => x.TokenHash)
                .IsRequired()
                .HasMaxLength(64);

            modelBuilder.Entity<RefreshToken>()
                .Property(x => x.ExpiresAt)
                .IsRequired();

            modelBuilder.Entity<RefreshToken>()
                .Property(x => x.CreatedAt)
                .IsRequired();

            modelBuilder.Entity<RefreshToken>()
                .Property(x => x.ReplacedByTokenHash)
                .HasMaxLength(64);

            modelBuilder.Entity<RefreshToken>()
                .Property(x => x.CreatedByIp)
                .HasMaxLength(64);

            modelBuilder.Entity<RefreshToken>()
                .Property(x => x.RevokedByIp)
                .HasMaxLength(64);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(x => x.TokenHash)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(x => x.UserId);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(x => x.ExpiresAt);

            modelBuilder.Entity<RefreshToken>()
                .HasOne(x => x.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefreshToken>()
                .HasQueryFilter(x => x.User != null && x.User.DeletedAt == null);

            // Keep the newsletter entity aligned with the existing table name/migration.
            modelBuilder.Entity<NewsletterSubscriber>()
                .ToTable("NewsletterSubscribers");

            modelBuilder.Entity<NewsletterSubscriber>()
                .HasKey(x => x.NewsletterID);

            modelBuilder.Entity<NewsletterSubscriber>()
                .Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(320);

            modelBuilder.Entity<NewsletterSubscriber>()
                .Property(x => x.SubscribedAt)
                .IsRequired();

            modelBuilder.Entity<NewsletterSubscriber>()
                .Property(x => x.IsActive)
                .IsRequired();

            modelBuilder.Entity<NewsletterSubscriber>()
                .HasIndex(x => x.Email)
                .IsUnique();
        }
    }
}
