# NileGuideApi Database ERD

Source of truth: `NileGuideApi/Data/AppDbContext.cs` and the current EF Core model snapshot.

Rendered SVG: [DATABASE_ERD-1.svg](./DATABASE_ERD-1.svg)

```mermaid
erDiagram
    USERS ||--|| USER_PROFILES : has
    USERS ||--o{ REFRESH_TOKENS : owns
    USERS ||--o{ PASSWORD_RESET_TOKENS : owns
    USERS ||--o{ WISHLIST_ITEMS : saves
    USERS ||--o{ PLAN_ITEMS : schedules
    USERS ||--o{ REVIEWS : writes
    USERS ||--o{ CHAT_CONVERSATIONS : owns

    CATEGORIES ||--o{ ACTIVITIES : groups
    CITIES ||--o{ ACTIVITIES : hosts

    ACTIVITIES ||--o{ ACTIVITY_IMAGES : has
    ACTIVITIES ||--o{ ACTIVITY_HOURS : opens
    ACTIVITIES ||--o{ BOOKING_LINKS : books_via
    ACTIVITIES ||--o{ WISHLIST_ITEMS : saved_as
    ACTIVITIES ||--o{ PLAN_ITEMS : planned_as
    ACTIVITIES ||--o{ REVIEWS : receives
    ACTIVITIES ||--o{ ACTIVITY_VIEWS : viewed_as

    USERS {
        int Id PK
        string Email UK
        string PasswordHash
        string FullName
        string Nationality
        date DateOfBirth
        string ProfilePictureUrl
        string Role
        bool IsActive
        datetime CreatedAt
        datetime UpdatedAt
        datetime DeletedAt
    }

    USER_PROFILES {
        int UserProfileId PK
        int UserId FK,UK
        bool HasTravelDates
        date TravelStartDate
        date TravelEndDate
        string BudgetLevel
        json PreferredCityIdsJson
        json InterestCategoryIdsJson
        datetime CreatedAt
        datetime UpdatedAt
    }

    CATEGORIES {
        int CategoryID PK
        string CategoryName
        string Description
        string IconName
        bool IsActive
        datetime CreatedAt
        datetime UpdatedAt
        datetime DeletedAt
    }

    CITIES {
        int CityID PK
        string CityName
        string Region
        decimal Latitude
        decimal Longitude
        bool IsPopular
        datetime CreatedAt
        datetime UpdatedAt
        datetime DeletedAt
    }

    ACTIVITIES {
        int ActivityID PK
        string ActivityName
        string Description
        int CategoryID FK
        int CityID FK
        decimal Price
        decimal MinPrice
        string PriceCurrency
        string PriceBasis
        int Duration
        string GroupSize
        string Cancellation
        string RequiredDocuments
        string Region
        float Latitude
        float Longitude
        float Rating
        int ReviewCount
        string ExternalId UK
        string Provider
        bool IsActive
        datetime CreatedAt
        datetime UpdatedAt
        datetime DeletedAt
    }

    ACTIVITY_IMAGES {
        int ImageID PK
        int ActivityID FK
        string Url
        bool IsPrimary
        int SortOrder
        datetime CreatedAt
        datetime UpdatedAt
        datetime DeletedAt
    }

    ACTIVITY_HOURS {
        int Id PK
        int ActivityID FK
        byte OpenHour
        string OpenAmPm
        byte CloseHour
        string CloseAmPm
    }

    BOOKING_LINKS {
        int Id PK
        int ActivityID FK
        string Provider
        string Url
    }

    WISHLIST_ITEMS {
        int Id PK
        int UserId FK
        int ActivityID FK
        datetime CreatedAtUtc
    }

    PLAN_ITEMS {
        int Id PK
        int UserId FK
        int ActivityId FK
        date ScheduledDate
        time StartTime
        datetime CreatedAtUtc
        datetime UpdatedAtUtc
    }

    REVIEWS {
        int ReviewId PK
        int ActivityId FK
        int UserId FK
        string ReviewerName
        string ReviewerCity
        int Rating
        string Comment
        datetime CreatedAt
        datetime UpdatedAt
        datetime DeletedAt
    }

    ACTIVITY_VIEWS {
        int Id PK
        int ActivityId FK
        datetime ViewedAt
    }

    REFRESH_TOKENS {
        int Id PK
        int UserId FK
        string TokenHash UK
        datetime ExpiresAt
        datetime CreatedAt
        datetime RevokedAt
        string ReplacedByTokenHash
        string CreatedByIp
        string RevokedByIp
    }

    PASSWORD_RESET_TOKENS {
        int Id PK
        int UserId FK
        string TokenHash
        datetime CreatedAt
        datetime ExpiresAt
        datetime UsedAt
        int AttemptCount
        datetime LastAttemptAt
    }

    CHAT_CONVERSATIONS {
        int UserId PK,FK
        string ConversationId PK
    }

    NEWSLETTER_SUBSCRIBERS {
        int NewsletterID PK
        string Email UK
        datetime SubscribedAt
        bool IsActive
    }
```

## Relationship Notes

- `UserProfiles.UserId` is unique, so each user has at most one profile.
- `WishlistItems` and `PlanItems` are join-style tables between `Users` and `Activities`.
- `Reviews.UserId` uses restrict delete behavior; most other user/activity dependent tables cascade.
- `NewsletterSubscribers` is standalone and is not linked to `Users`.
- `UserProfiles.PreferredCityIdsJson` and `UserProfiles.InterestCategoryIdsJson` store IDs as JSON arrays, not enforced foreign keys.

## Important Indexes And Constraints

- `Users.Email` is unique.
- `Activities.ExternalId` is unique when it is not null.
- `RefreshTokens.TokenHash` is unique.
- `NewsletterSubscribers.Email` is unique.
- `WishlistItems` has a unique composite index on `(UserId, ActivityID)`.
- `PlanItems` has a unique composite index on `(UserId, ActivityId)`.
- `ChatConversations` uses composite primary key `(UserId, ConversationId)`.
- `ActivityHours` constrains `OpenHour` and `CloseHour` to `1..12`, and `OpenAmPm` / `CloseAmPm` to `AM` or `PM`.
