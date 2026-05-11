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

        // Extra profile preferences for each user.
        public DbSet<UserProfile> UserProfiles { get; set; } = null!;

        // Static content categories configured by the admin/domain model.
        public DbSet<Category> Categories { get; set; } = null!;

        // Cities supported by the app's content and discovery flows.
        public DbSet<City> Cities { get; set; } = null!;

        // Discoverable activities tied to a category and a city.
        public DbSet<Activity> Activities { get; set; } = null!;

        // Ordered images that belong to an activity.
        public DbSet<ActivityImage> ActivityImages { get; set; } = null!;

        // Opening and closing windows for each activity.
        public DbSet<ActivityHour> ActivityHours { get; set; } = null!;

        // Provider booking links that belong to an activity.
        public DbSet<BookingLink> BookingLinks { get; set; } = null!;

        // Password reset attempts and issued reset codes.
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;

        // Rotating refresh tokens back the session lifecycle.
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        // Newsletter subscriptions are stored independently from user accounts.
        public DbSet<NewsletterSubscriber> NewsletterSubscribers { get; set; } = null!;

        // Saved activity wishlist entries for authenticated users.
        public DbSet<WishlistItem> WishlistItems { get; set; } = null!;

        // Scheduled activity plan entries for authenticated users.
        public DbSet<PlanItem> PlanItems { get; set; } = null!;

        // Reviews written by authenticated users for activities.
        public DbSet<Review> Reviews { get; set; } = null!;

        // View logs for activity details pages.
        public DbSet<ActivityView> ActivityViews { get; set; } = null!;

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

            modelBuilder.Entity<User>()
                .Property(u => u.Id)
                .HasColumnOrder(1);

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .HasColumnOrder(2);

            modelBuilder.Entity<User>()
                .Property(u => u.PasswordHash)
                .HasColumnOrder(3);

            modelBuilder.Entity<User>()
                .Property(u => u.FullName)
                .HasColumnOrder(4);

            modelBuilder.Entity<User>()
                .Property(u => u.Nationality)
                .HasColumnOrder(5);

            modelBuilder.Entity<User>()
                .Property(u => u.DateOfBirth)
                .IsRequired()
                .HasColumnName("date_of_birth")
                .HasColumnType("date")
                .HasColumnOrder(6);

            modelBuilder.Entity<User>()
                .Property(u => u.ProfilePictureUrl)
                .HasColumnName("profile_picture_url")
                .HasMaxLength(500)
                .HasColumnType("varchar(500)")
                .HasColumnOrder(7);

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasColumnOrder(8);

            modelBuilder.Entity<User>()
                .Property(u => u.IsActive)
                .HasColumnOrder(9);

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasColumnOrder(10);

            modelBuilder.Entity<User>()
                .Property(u => u.UpdatedAt)
                .HasColumnOrder(11);

            modelBuilder.Entity<User>()
                .Property(u => u.DeletedAt)
                .HasColumnOrder(12);

            modelBuilder.Entity<User>()
                .HasMany(u => u.WishlistItems)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.PlanItems)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User profile table.
            // One user has one profile.
            modelBuilder.Entity<UserProfile>()
                .ToTable("UserProfiles");

            modelBuilder.Entity<UserProfile>()
                .HasKey(x => x.UserProfileId);

            modelBuilder.Entity<UserProfile>()
                .Property(x => x.UserProfileId)
                .HasColumnOrder(1);

            modelBuilder.Entity<UserProfile>()
                .Property(x => x.UserId)
                .IsRequired()
                .HasColumnOrder(2);

            modelBuilder.Entity<UserProfile>()
                .HasIndex(x => x.UserId)
                .IsUnique();

            modelBuilder.Entity<UserProfile>()
                .HasOne(x => x.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<UserProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserProfile>()
                .HasQueryFilter(x => x.User != null && x.User.DeletedAt == null);

            modelBuilder.Entity<UserProfile>()
                .Property(x => x.HasTravelDates)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnOrder(3);

            modelBuilder.Entity<UserProfile>()
                .Property(x => x.TravelStartDate)
                .IsRequired()
                .HasColumnType("date")
                .HasDefaultValue(DateOnly.MinValue)
                .HasColumnOrder(4);

            modelBuilder.Entity<UserProfile>()
                .Property(x => x.TravelEndDate)
                .IsRequired()
                .HasColumnType("date")
                .HasDefaultValue(DateOnly.MinValue)
                .HasColumnOrder(5);

            modelBuilder.Entity<UserProfile>()
                .Property(x => x.BudgetLevel)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("varchar(50)")
                .HasDefaultValue("")
                .HasColumnOrder(6);

            modelBuilder.Entity<UserProfile>()
                .Property(x => x.PreferredCityIdsJson)
                .IsRequired()
                .HasColumnType("nvarchar(max)")
                .HasDefaultValue("[]")
                .HasColumnOrder(7);

            modelBuilder.Entity<UserProfile>()
                .Property(x => x.InterestCategoryIdsJson)
                .IsRequired()
                .HasColumnType("nvarchar(max)")
                .HasDefaultValue("[]")
                .HasColumnOrder(8);

            modelBuilder.Entity<UserProfile>()
                .Property(x => x.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()")
                .HasColumnOrder(9);

            modelBuilder.Entity<UserProfile>()
                .Property(x => x.UpdatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()")
                .HasColumnOrder(10);

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
                .ToTable("Activities");

            modelBuilder.Entity<Activity>()
                .HasKey(x => x.ActivityID);

            modelBuilder.Entity<Activity>()
                .Property(x => x.ActivityID)
                .HasColumnOrder(1);

            modelBuilder.Entity<Activity>()
                .Property(x => x.ActivityName)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnType("nvarchar(255)")
                .HasColumnOrder(2);

            modelBuilder.Entity<Activity>()
                .Property(x => x.Description)
                .HasColumnType("nvarchar(max)")
                .HasColumnOrder(3);

            modelBuilder.Entity<Activity>()
                .Property(x => x.CategoryID)
                .HasColumnOrder(4);

            modelBuilder.Entity<Activity>()
                .Property(x => x.CityID)
                .HasColumnOrder(5);

            modelBuilder.Entity<Activity>()
                .Property(x => x.Price)
                .HasColumnType("decimal(10,2)")
                .HasColumnOrder(6);

            modelBuilder.Entity<Activity>()
                .Property(x => x.MinPrice)
                .HasColumnType("decimal(10,2)")
                .HasColumnOrder(7);

            modelBuilder.Entity<Activity>()
                .Property(x => x.PriceCurrency)
                .IsRequired()
                .HasMaxLength(10)
                .HasColumnType("nvarchar(10)")
                .HasDefaultValue("USD")
                .HasColumnOrder(8);

            modelBuilder.Entity<Activity>()
                .Property(x => x.PriceBasis)
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)")
                .HasColumnOrder(9);

            modelBuilder.Entity<Activity>()
                .Property(x => x.Duration)
                .IsRequired()
                .HasColumnOrder(10);

            modelBuilder.Entity<Activity>()
                .Property(x => x.GroupSize)
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)")
                .HasColumnOrder(11);

            modelBuilder.Entity<Activity>()
                .Property(x => x.Cancellation)
                .HasColumnType("nvarchar(max)")
                .HasColumnOrder(12);

            modelBuilder.Entity<Activity>()
                .Property(x => x.RequiredDocuments)
                .HasColumnType("nvarchar(max)")
                .HasColumnOrder(13);

            modelBuilder.Entity<Activity>()
                .Property(x => x.Region)
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)")
                .HasColumnOrder(14);

            modelBuilder.Entity<Activity>()
                .Property(x => x.Latitude)
                .HasColumnType("float")
                .HasColumnOrder(15);

            modelBuilder.Entity<Activity>()
                .Property(x => x.Longitude)
                .HasColumnType("float")
                .HasColumnOrder(16);

            modelBuilder.Entity<Activity>()
                .Property(x => x.Rating)
                .IsRequired()
                .HasColumnType("float")
                .HasColumnOrder(17);

            modelBuilder.Entity<Activity>()
                .Property(x => x.ReviewCount)
                .IsRequired()
                .HasColumnOrder(18);

            modelBuilder.Entity<Activity>()
                .Property(x => x.ExternalId)
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)")
                .HasColumnOrder(19);

            modelBuilder.Entity<Activity>()
                .HasIndex(x => x.ExternalId)
                .IsUnique()
                .HasFilter("[ExternalId] IS NOT NULL");

            modelBuilder.Entity<Activity>()
                .Property(x => x.Provider)
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)")
                .HasColumnOrder(20);

            modelBuilder.Entity<Activity>()
                .Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true)
                .HasColumnOrder(21);

            modelBuilder.Entity<Activity>()
                .Property(x => x.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()")
                .HasColumnOrder(22);

            modelBuilder.Entity<Activity>()
                .Property(x => x.UpdatedAt)
                .IsRequired()
                .HasColumnType("datetime")
                .HasColumnOrder(23);

            modelBuilder.Entity<Activity>()
                .Property(x => x.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnOrder(24);

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
                .HasMany(x => x.WishlistItems)
                .WithOne(x => x.Activity)
                .HasForeignKey(x => x.ActivityID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Activity>()
                .HasMany(x => x.PlanItems)
                .WithOne(x => x.Activity)
                .HasForeignKey(x => x.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Activity>()
                .HasQueryFilter(x => x.DeletedAt == null);

            modelBuilder.Entity<WishlistItem>()
                .ToTable("WishlistItems");

            modelBuilder.Entity<WishlistItem>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<WishlistItem>()
                .Property(x => x.UserId)
                .IsRequired();

            modelBuilder.Entity<WishlistItem>()
                .Property(x => x.ActivityID)
                .IsRequired();

            modelBuilder.Entity<WishlistItem>()
                .Property(x => x.CreatedAtUtc)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<WishlistItem>()
                .HasIndex(x => new { x.UserId, x.ActivityID })
                .IsUnique();

            modelBuilder.Entity<WishlistItem>()
                .HasIndex(x => x.ActivityID);

            modelBuilder.Entity<WishlistItem>()
                .HasQueryFilter(x =>
                    x.User != null &&
                    x.User.DeletedAt == null &&
                    x.Activity != null &&
                    x.Activity.DeletedAt == null);

            modelBuilder.Entity<PlanItem>()
                .ToTable("PlanItems");

            modelBuilder.Entity<PlanItem>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<PlanItem>()
                .Property(x => x.UserId)
                .IsRequired();

            modelBuilder.Entity<PlanItem>()
                .Property(x => x.ActivityId)
                .IsRequired();

            modelBuilder.Entity<PlanItem>()
                .Property(x => x.ScheduledDate)
                .IsRequired()
                .HasColumnType("date");

            modelBuilder.Entity<PlanItem>()
                .Property(x => x.StartTime)
                .IsRequired()
                .HasColumnType("time");

            modelBuilder.Entity<PlanItem>()
                .Property(x => x.CreatedAtUtc)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<PlanItem>()
                .Property(x => x.UpdatedAtUtc)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<PlanItem>()
                .HasIndex(x => new { x.UserId, x.ActivityId })
                .IsUnique();

            modelBuilder.Entity<PlanItem>()
                .HasIndex(x => x.ActivityId);

            modelBuilder.Entity<PlanItem>()
                .HasQueryFilter(x =>
                    x.User != null &&
                    x.User.DeletedAt == null &&
                    x.Activity != null &&
                    x.Activity.DeletedAt == null);

            modelBuilder.Entity<Review>()
                .ToTable("Reviews");

            modelBuilder.Entity<Review>()
                .HasKey(x => x.ReviewId);

            modelBuilder.Entity<Review>()
                .Property(x => x.ReviewId)
                .HasColumnOrder(1);

            modelBuilder.Entity<Review>()
                .Property(x => x.ActivityId)
                .IsRequired()
                .HasColumnOrder(2);

            modelBuilder.Entity<Review>()
                .Property(x => x.UserId)
                .IsRequired()
                .HasColumnOrder(3);

            modelBuilder.Entity<Review>()
                .Property(x => x.ReviewerName)
                .IsRequired()
                .HasMaxLength(150)
                .HasColumnType("nvarchar(150)")
                .HasColumnOrder(4);

            modelBuilder.Entity<Review>()
                .Property(x => x.ReviewerCity)
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)")
                .HasColumnOrder(5);

            modelBuilder.Entity<Review>()
                .Property(x => x.Rating)
                .IsRequired()
                .HasColumnOrder(6);

            modelBuilder.Entity<Review>()
                .Property(x => x.Comment)
                .IsRequired()
                .HasColumnType("nvarchar(2000)")
                .HasColumnOrder(7);

            modelBuilder.Entity<Review>()
                .Property(x => x.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnOrder(8);

            modelBuilder.Entity<Review>()
                .Property(x => x.UpdatedAt)
                .HasColumnType("datetime2")
                .HasColumnOrder(9);

            modelBuilder.Entity<Review>()
                .Property(x => x.DeletedAt)
                .HasColumnType("datetime2")
                .HasColumnOrder(10);

            modelBuilder.Entity<Review>()
                .HasOne(x => x.Activity)
                .WithMany()
                .HasForeignKey(x => x.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasIndex(x => x.ActivityId);

            modelBuilder.Entity<Review>()
                .HasIndex(x => x.UserId);

            modelBuilder.Entity<Review>()
                .HasQueryFilter(x =>
                    x.DeletedAt == null &&
                    x.Activity != null &&
                    x.Activity.DeletedAt == null &&
                    x.User != null &&
                    x.User.DeletedAt == null);

            modelBuilder.Entity<ActivityView>()
                .ToTable("ActivityViews");

            modelBuilder.Entity<ActivityView>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<ActivityView>()
                .Property(x => x.Id)
                .HasColumnOrder(1);

            modelBuilder.Entity<ActivityView>()
                .Property(x => x.ActivityId)
                .IsRequired()
                .HasColumnOrder(2);

            modelBuilder.Entity<ActivityView>()
                .Property(x => x.ViewedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnOrder(3);

            modelBuilder.Entity<ActivityView>()
                .HasOne(x => x.Activity)
                .WithMany()
                .HasForeignKey(x => x.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ActivityView>()
                .HasIndex(x => x.ActivityId);

            modelBuilder.Entity<ActivityView>()
                .HasIndex(x => x.ViewedAt);

            modelBuilder.Entity<ActivityView>()
                .HasQueryFilter(x => x.Activity != null && x.Activity.DeletedAt == null);

            modelBuilder.Entity<ActivityHour>()
                .ToTable("ActivityHours", tableBuilder =>
                {
                    tableBuilder.HasCheckConstraint(
                        "CK_ActivityHours_OpenHour",
                        "[open_hour] BETWEEN 1 AND 12");

                    tableBuilder.HasCheckConstraint(
                        "CK_ActivityHours_CloseHour",
                        "[close_hour] BETWEEN 1 AND 12");

                    tableBuilder.HasCheckConstraint(
                        "CK_ActivityHours_OpenAmPm",
                        "[open_ampm] IN ('AM', 'PM')");

                    tableBuilder.HasCheckConstraint(
                        "CK_ActivityHours_CloseAmPm",
                        "[close_ampm] IN ('AM', 'PM')");
                });

            modelBuilder.Entity<ActivityHour>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<ActivityHour>()
                .Property(x => x.Id)
                .HasColumnName("id");

            modelBuilder.Entity<ActivityHour>()
                .Property(x => x.ActivityID)
                .HasColumnName("ActivityID");

            modelBuilder.Entity<ActivityHour>()
                .Property(x => x.OpenHour)
                .HasColumnName("open_hour")
                .HasColumnType("tinyint")
                .IsRequired();

            modelBuilder.Entity<ActivityHour>()
                .Property(x => x.OpenAmPm)
                .HasColumnName("open_ampm")
                .HasMaxLength(2)
                .HasColumnType("varchar(2)")
                .IsRequired();

            modelBuilder.Entity<ActivityHour>()
                .Property(x => x.CloseHour)
                .HasColumnName("close_hour")
                .HasColumnType("tinyint")
                .IsRequired();

            modelBuilder.Entity<ActivityHour>()
                .Property(x => x.CloseAmPm)
                .HasColumnName("close_ampm")
                .HasMaxLength(2)
                .HasColumnType("varchar(2)")
                .IsRequired();

            modelBuilder.Entity<ActivityHour>()
                .HasOne(x => x.Activity)
                .WithMany(x => x.ActivityHours)
                .HasForeignKey(x => x.ActivityID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ActivityHour>()
                .HasIndex(x => x.ActivityID);

            modelBuilder.Entity<ActivityHour>()
                .HasQueryFilter(x => x.Activity != null && x.Activity.DeletedAt == null);

            modelBuilder.Entity<ActivityImage>()
                .ToTable("ActivityImages");

            modelBuilder.Entity<ActivityImage>()
                .HasKey(x => x.ImageID);

            modelBuilder.Entity<ActivityImage>()
                .Property(x => x.ImageID)
                .HasColumnOrder(1);

            modelBuilder.Entity<ActivityImage>()
                .Property(x => x.ActivityID)
                .HasColumnOrder(2);

            modelBuilder.Entity<ActivityImage>()
                .Property(x => x.Url)
                .IsRequired()
                .HasColumnType("nvarchar(max)")
                .HasColumnOrder(3);

            modelBuilder.Entity<ActivityImage>()
                .Property(x => x.IsPrimary)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnOrder(4);

            modelBuilder.Entity<ActivityImage>()
                .Property(x => x.SortOrder)
                .IsRequired()
                .HasColumnOrder(5);

            modelBuilder.Entity<ActivityImage>()
                .Property(x => x.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()")
                .HasColumnOrder(6);

            modelBuilder.Entity<ActivityImage>()
                .Property(x => x.UpdatedAt)
                .IsRequired()
                .HasColumnType("datetime")
                .HasColumnOrder(7);

            modelBuilder.Entity<ActivityImage>()
                .Property(x => x.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnOrder(8);

            modelBuilder.Entity<ActivityImage>()
                .HasOne(x => x.Activity)
                .WithMany(x => x.ActivityImages)
                .HasForeignKey(x => x.ActivityID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ActivityImage>()
                .HasIndex(x => x.ActivityID);

            modelBuilder.Entity<ActivityImage>()
                .HasQueryFilter(x => x.DeletedAt == null);

            modelBuilder.Entity<BookingLink>()
                .ToTable("BookingLinks");

            modelBuilder.Entity<BookingLink>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<BookingLink>()
                .Property(x => x.Provider)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");

            modelBuilder.Entity<BookingLink>()
                .Property(x => x.Url)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            modelBuilder.Entity<BookingLink>()
                .HasOne(x => x.Activity)
                .WithMany(x => x.BookingLinks)
                .HasForeignKey(x => x.ActivityID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookingLink>()
                .HasIndex(x => x.ActivityID);

            modelBuilder.Entity<BookingLink>()
                .HasQueryFilter(x => x.Activity != null && x.Activity.DeletedAt == null);

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