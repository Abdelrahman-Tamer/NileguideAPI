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

        // Static content categories configured by the admin/domain model.
        public DbSet<Category> Categories { get; set; } = null!;

        // Cities supported by the app's content and discovery flows.
        public DbSet<City> Cities { get; set; } = null!;

        // Discoverable activities tied to a category and a city.
        public DbSet<Activity> Activities { get; set; } = null!;

        // Ordered images that belong to an activity.
        public DbSet<ActivityImage> ActivityImages { get; set; } = null!;

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

            modelBuilder.Entity<Category>()
                .ToTable("Categories");

            modelBuilder.Entity<Category>()
                .HasKey(x => x.CategoryID);

            modelBuilder.Entity<Category>()
                .Property(x => x.CategoryName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");

            modelBuilder.Entity<Category>()
                .Property(x => x.Description)
                .HasMaxLength(500)
                .HasColumnType("varchar(500)");

            modelBuilder.Entity<Category>()
                .Property(x => x.IconName)
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");

            modelBuilder.Entity<Category>()
                .Property(x => x.IsActive)
                .IsRequired();

            modelBuilder.Entity<Category>()
                .Property(x => x.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime");

            modelBuilder.Entity<Category>()
                .Property(x => x.UpdatedAt)
                .IsRequired()
                .HasColumnType("datetime");

            modelBuilder.Entity<Category>()
                .Property(x => x.DeletedAt)
                .HasColumnType("datetime");

            modelBuilder.Entity<Category>()
                .HasQueryFilter(x => x.DeletedAt == null);

            modelBuilder.Entity<City>()
                .ToTable("Cities");

            modelBuilder.Entity<City>()
                .HasKey(x => x.CityID);

            modelBuilder.Entity<City>()
                .Property(x => x.CityName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");

            modelBuilder.Entity<City>()
                .Property(x => x.Region)
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");

            modelBuilder.Entity<City>()
                .Property(x => x.Latitude)
                .HasColumnType("decimal(10,7)");

            modelBuilder.Entity<City>()
                .Property(x => x.Longitude)
                .HasColumnType("decimal(10,7)");

            modelBuilder.Entity<City>()
                .Property(x => x.IsPopular)
                .IsRequired();

            modelBuilder.Entity<City>()
                .Property(x => x.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime");

            modelBuilder.Entity<City>()
                .Property(x => x.UpdatedAt)
                .IsRequired()
                .HasColumnType("datetime");

            modelBuilder.Entity<City>()
                .Property(x => x.DeletedAt)
                .HasColumnType("datetime");

            modelBuilder.Entity<City>()
                .HasQueryFilter(x => x.DeletedAt == null);

            modelBuilder.Entity<Activity>()
                .ToTable("Activities", tableBuilder =>
                {
                    tableBuilder.HasCheckConstraint(
                        "CK_Activities_Status_Allowed",
                        "[Status] IN ('Available', 'Unavailable', 'Temporarily Closed')");

                    tableBuilder.HasCheckConstraint(
                        "CK_Activities_PriceBasis_Allowed",
                        "[PriceBasis] IS NULL OR [PriceBasis] IN ('Free', 'PerPerson', 'PerTicket', 'PerHour', 'PerTrip', 'PerNight')");
                });

            modelBuilder.Entity<Activity>()
                .HasKey(x => x.ActivityID);

            modelBuilder.Entity<Activity>()
                .Property(x => x.ActivityName)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            modelBuilder.Entity<Activity>()
                .Property(x => x.Description)
                .HasColumnType("varchar(max)");

            modelBuilder.Entity<Activity>()
                .Property(x => x.PriceMinEst)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Activity>()
                .Property(x => x.PriceMaxEst)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Activity>()
                .Property(x => x.PriceCurrency)
                .IsRequired()
                .HasMaxLength(10)
                .HasColumnType("varchar(10)")
                .HasDefaultValue("USD");

            modelBuilder.Entity<Activity>()
                .Property(x => x.PriceBasis)
                .HasMaxLength(20)
                .HasColumnType("varchar(20)");

            modelBuilder.Entity<Activity>()
                .Property(x => x.OpeningHours)
                .HasMaxLength(200)
                .HasColumnType("nvarchar(200)");

            modelBuilder.Entity<Activity>()
                .Property(x => x.Location)
                .HasMaxLength(255)
                .HasColumnType("varchar(255)");

            modelBuilder.Entity<Activity>()
                .Property(x => x.Latitude)
                .HasColumnType("decimal(10,7)");

            modelBuilder.Entity<Activity>()
                .Property(x => x.Longitude)
                .HasColumnType("decimal(10,7)");

            modelBuilder.Entity<Activity>()
                .Property(x => x.Rating)
                .IsRequired()
                .HasColumnType("decimal(3,2)");

            modelBuilder.Entity<Activity>()
                .Property(x => x.ReviewCount)
                .IsRequired();

            modelBuilder.Entity<Activity>()
                .Property(x => x.RequiresPersonalID)
                .IsRequired()
                .HasDefaultValue(false);

            modelBuilder.Entity<Activity>()
                .Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("varchar(20)")
                .HasDefaultValue("Available");

            modelBuilder.Entity<Activity>()
                .Property(x => x.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime");

            modelBuilder.Entity<Activity>()
                .Property(x => x.UpdatedAt)
                .IsRequired()
                .HasColumnType("datetime");

            modelBuilder.Entity<Activity>()
                .Property(x => x.DeletedAt)
                .HasColumnType("datetime");

            modelBuilder.Entity<Activity>()
                .HasOne(x => x.Category)
                .WithMany(x => x.Activities)
                .HasForeignKey(x => x.CategoryID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Activity>()
                .HasOne(x => x.City)
                .WithMany(x => x.Activities)
                .HasForeignKey(x => x.CityID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Activity>()
                .HasQueryFilter(x => x.DeletedAt == null);

            modelBuilder.Entity<ActivityImage>()
                .ToTable("ActivityImages");

            modelBuilder.Entity<ActivityImage>()
                .HasKey(x => x.ImageID);

            modelBuilder.Entity<ActivityImage>()
                .Property(x => x.Url)
                .IsRequired()
                .HasMaxLength(1000)
                .HasColumnType("varchar(1000)");

            modelBuilder.Entity<ActivityImage>()
                .Property(x => x.IsPrimary)
                .IsRequired();

            modelBuilder.Entity<ActivityImage>()
                .Property(x => x.SortOrder)
                .IsRequired();

            modelBuilder.Entity<ActivityImage>()
                .Property(x => x.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime");

            modelBuilder.Entity<ActivityImage>()
                .Property(x => x.UpdatedAt)
                .IsRequired()
                .HasColumnType("datetime");

            modelBuilder.Entity<ActivityImage>()
                .Property(x => x.DeletedAt)
                .HasColumnType("datetime");

            modelBuilder.Entity<ActivityImage>()
                .HasOne(x => x.Activity)
                .WithMany(x => x.ActivityImages)
                .HasForeignKey(x => x.ActivityID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ActivityImage>()
                .HasQueryFilter(x => x.DeletedAt == null);

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
