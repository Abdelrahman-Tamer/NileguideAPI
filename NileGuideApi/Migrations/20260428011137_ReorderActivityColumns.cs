using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NileGuideApi.Migrations
{
    /// <inheritdoc />
    public partial class ReorderActivityColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(BuildReorderSql(
                """
                [ActivityID] int NOT NULL IDENTITY(1,1),
                [ActivityName] nvarchar(255) NOT NULL,
                [Description] nvarchar(max) NULL,
                [CategoryID] int NOT NULL,
                [CityID] int NOT NULL,
                [Price] decimal(10,2) NULL,
                [MinPrice] decimal(10,2) NULL,
                [PriceCurrency] nvarchar(10) NOT NULL DEFAULT N'USD',
                [PriceBasis] nvarchar(50) NULL,
                [Duration] int NOT NULL DEFAULT 0,
                [GroupSize] nvarchar(100) NULL,
                [Cancellation] nvarchar(max) NULL,
                [RequiredDocuments] nvarchar(max) NULL,
                [Region] nvarchar(100) NULL,
                [Latitude] float NULL,
                [Longitude] float NULL,
                [Rating] float NOT NULL,
                [ReviewCount] int NOT NULL,
                [ExternalId] nvarchar(100) NULL,
                [Provider] nvarchar(50) NULL,
                [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
                [CreatedAt] datetime NOT NULL DEFAULT GETDATE(),
                [UpdatedAt] datetime NOT NULL,
                [DeletedAt] datetime NULL
                """,
                """
                [ActivityID], [ActivityName], [Description], [CategoryID], [CityID],
                [Price], [MinPrice], [PriceCurrency], [PriceBasis], [Duration],
                [GroupSize], [Cancellation], [RequiredDocuments], [Region],
                [Latitude], [Longitude], [Rating], [ReviewCount], [ExternalId],
                [Provider], [IsActive], [CreatedAt], [UpdatedAt], [DeletedAt]
                """));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(BuildReorderSql(
                """
                [ActivityID] int NOT NULL IDENTITY(1,1),
                [ActivityName] nvarchar(255) NOT NULL,
                [Description] nvarchar(max) NULL,
                [CategoryID] int NOT NULL,
                [CityID] int NOT NULL,
                [Price] decimal(10,2) NULL,
                [MinPrice] decimal(10,2) NULL,
                [PriceCurrency] nvarchar(10) NOT NULL DEFAULT N'USD',
                [PriceBasis] nvarchar(50) NULL,
                [GroupSize] nvarchar(100) NULL,
                [Cancellation] nvarchar(max) NULL,
                [RequiredDocuments] nvarchar(max) NULL,
                [Region] nvarchar(100) NULL,
                [Latitude] float NULL,
                [Longitude] float NULL,
                [Rating] float NOT NULL,
                [ReviewCount] int NOT NULL,
                [ExternalId] nvarchar(100) NULL,
                [Provider] nvarchar(50) NULL,
                [CreatedAt] datetime NOT NULL DEFAULT GETDATE(),
                [UpdatedAt] datetime NOT NULL,
                [DeletedAt] datetime NULL,
                [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
                [Duration] int NOT NULL DEFAULT 0
                """,
                """
                [ActivityID], [ActivityName], [Description], [CategoryID], [CityID],
                [Price], [MinPrice], [PriceCurrency], [PriceBasis], [GroupSize],
                [Cancellation], [RequiredDocuments], [Region], [Latitude],
                [Longitude], [Rating], [ReviewCount], [ExternalId], [Provider],
                [CreatedAt], [UpdatedAt], [DeletedAt], [IsActive], [Duration]
                """));
        }

        private static string BuildReorderSql(string columnDefinitions, string orderedColumns)
        {
            return $$"""
IF OBJECT_ID(N'[dbo].[Activities_Reordered]', N'U') IS NOT NULL
    DROP TABLE [dbo].[Activities_Reordered];

IF OBJECT_ID(N'[dbo].[FK_ActivityImages_Activities_ActivityID]', N'F') IS NOT NULL
    ALTER TABLE [dbo].[ActivityImages] DROP CONSTRAINT [FK_ActivityImages_Activities_ActivityID];

IF OBJECT_ID(N'[dbo].[FK_ActivityHours_Activities_ActivityID]', N'F') IS NOT NULL
    ALTER TABLE [dbo].[ActivityHours] DROP CONSTRAINT [FK_ActivityHours_Activities_ActivityID];

IF OBJECT_ID(N'[dbo].[FK_ActivityHours_Activities_activity_id]', N'F') IS NOT NULL
    ALTER TABLE [dbo].[ActivityHours] DROP CONSTRAINT [FK_ActivityHours_Activities_activity_id];

IF OBJECT_ID(N'[dbo].[FK_BookingLinks_Activities_ActivityID]', N'F') IS NOT NULL
    ALTER TABLE [dbo].[BookingLinks] DROP CONSTRAINT [FK_BookingLinks_Activities_ActivityID];

IF OBJECT_ID(N'[dbo].[FK_WishlistItems_Activities_ActivityID]', N'F') IS NOT NULL
    ALTER TABLE [dbo].[WishlistItems] DROP CONSTRAINT [FK_WishlistItems_Activities_ActivityID];

CREATE TABLE [dbo].[Activities_Reordered]
(
{{columnDefinitions}},
    CONSTRAINT [PK_Activities_Reordered] PRIMARY KEY CLUSTERED ([ActivityID] ASC),
    CONSTRAINT [FK_Activities_Reordered_Categories_CategoryID] FOREIGN KEY ([CategoryID])
        REFERENCES [dbo].[Categories] ([CategoryID]) ON DELETE CASCADE,
    CONSTRAINT [FK_Activities_Reordered_Cities_CityID] FOREIGN KEY ([CityID])
        REFERENCES [dbo].[Cities] ([CityID]) ON DELETE CASCADE
);

SET IDENTITY_INSERT [dbo].[Activities_Reordered] ON;

INSERT INTO [dbo].[Activities_Reordered] ({{orderedColumns}})
SELECT {{orderedColumns}}
FROM [dbo].[Activities];

SET IDENTITY_INSERT [dbo].[Activities_Reordered] OFF;

DROP TABLE [dbo].[Activities];

EXEC sp_rename N'[dbo].[Activities_Reordered]', N'Activities';

ALTER TABLE [dbo].[Activities] DROP CONSTRAINT [PK_Activities_Reordered];
ALTER TABLE [dbo].[Activities] ADD CONSTRAINT [PK_Activities] PRIMARY KEY CLUSTERED ([ActivityID] ASC);

ALTER TABLE [dbo].[Activities] DROP CONSTRAINT [FK_Activities_Reordered_Categories_CategoryID];
ALTER TABLE [dbo].[Activities]
    ADD CONSTRAINT [FK_Activities_Categories_CategoryID]
    FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[Categories] ([CategoryID]) ON DELETE CASCADE;

ALTER TABLE [dbo].[Activities] DROP CONSTRAINT [FK_Activities_Reordered_Cities_CityID];
ALTER TABLE [dbo].[Activities]
    ADD CONSTRAINT [FK_Activities_Cities_CityID]
    FOREIGN KEY ([CityID]) REFERENCES [dbo].[Cities] ([CityID]) ON DELETE CASCADE;

CREATE INDEX [IX_Activities_CategoryID] ON [dbo].[Activities] ([CategoryID]);
CREATE INDEX [IX_Activities_CityID] ON [dbo].[Activities] ([CityID]);
CREATE UNIQUE INDEX [IX_Activities_ExternalId]
    ON [dbo].[Activities] ([ExternalId])
    WHERE [ExternalId] IS NOT NULL;

IF OBJECT_ID(N'[dbo].[ActivityImages]', N'U') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[ActivityImages] WITH CHECK
        ADD CONSTRAINT [FK_ActivityImages_Activities_ActivityID]
        FOREIGN KEY ([ActivityID]) REFERENCES [dbo].[Activities] ([ActivityID]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[ActivityImages] CHECK CONSTRAINT [FK_ActivityImages_Activities_ActivityID];
END;

IF OBJECT_ID(N'[dbo].[ActivityHours]', N'U') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[ActivityHours] WITH CHECK
        ADD CONSTRAINT [FK_ActivityHours_Activities_ActivityID]
        FOREIGN KEY ([ActivityID]) REFERENCES [dbo].[Activities] ([ActivityID]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[ActivityHours] CHECK CONSTRAINT [FK_ActivityHours_Activities_ActivityID];
END;

IF OBJECT_ID(N'[dbo].[BookingLinks]', N'U') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[BookingLinks] WITH CHECK
        ADD CONSTRAINT [FK_BookingLinks_Activities_ActivityID]
        FOREIGN KEY ([ActivityID]) REFERENCES [dbo].[Activities] ([ActivityID]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[BookingLinks] CHECK CONSTRAINT [FK_BookingLinks_Activities_ActivityID];
END;

IF OBJECT_ID(N'[dbo].[WishlistItems]', N'U') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[WishlistItems] WITH CHECK
        ADD CONSTRAINT [FK_WishlistItems_Activities_ActivityID]
        FOREIGN KEY ([ActivityID]) REFERENCES [dbo].[Activities] ([ActivityID]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[WishlistItems] CHECK CONSTRAINT [FK_WishlistItems_Activities_ActivityID];
END;
""";
        }
    }
}
