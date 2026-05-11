using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NileGuideApi.Migrations
{
    /// <inheritdoc />
    public partial class ReorderUserColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(BuildReorderSql(
                """
                [Id] int NOT NULL IDENTITY(1,1),
                [Email] nvarchar(450) NOT NULL,
                [PasswordHash] nvarchar(max) NOT NULL,
                [FullName] nvarchar(max) NOT NULL,
                [Nationality] nvarchar(max) NOT NULL,
                [date_of_birth] date NULL,
                [profile_picture_url] varchar(500) NULL,
                [Role] nvarchar(max) NOT NULL,
                [IsActive] bit NOT NULL,
                [CreatedAt] datetime2 NOT NULL,
                [UpdatedAt] datetime2 NULL,
                [DeletedAt] datetime2 NULL
                """,
                """
                [Id], [Email], [PasswordHash], [FullName], [Nationality],
                [date_of_birth], [profile_picture_url], [Role], [IsActive],
                [CreatedAt], [UpdatedAt], [DeletedAt]
                """));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(BuildReorderSql(
                """
                [Id] int NOT NULL IDENTITY(1,1),
                [Email] nvarchar(450) NOT NULL,
                [PasswordHash] nvarchar(max) NOT NULL,
                [FullName] nvarchar(max) NOT NULL,
                [Nationality] nvarchar(max) NOT NULL,
                [Role] nvarchar(max) NOT NULL,
                [IsActive] bit NOT NULL,
                [CreatedAt] datetime2 NOT NULL,
                [UpdatedAt] datetime2 NULL,
                [DeletedAt] datetime2 NULL,
                [profile_picture_url] varchar(500) NULL,
                [date_of_birth] date NULL
                """,
                """
                [Id], [Email], [PasswordHash], [FullName], [Nationality],
                [Role], [IsActive], [CreatedAt], [UpdatedAt], [DeletedAt],
                [profile_picture_url], [date_of_birth]
                """));
        }

        private static string BuildReorderSql(string columnDefinitions, string orderedColumns)
        {
            return $$"""
IF OBJECT_ID(N'[dbo].[Users_Reordered]', N'U') IS NOT NULL
    DROP TABLE [dbo].[Users_Reordered];

IF OBJECT_ID(N'[dbo].[FK_PasswordResetTokens_Users_UserId]', N'F') IS NOT NULL
    ALTER TABLE [dbo].[PasswordResetTokens] DROP CONSTRAINT [FK_PasswordResetTokens_Users_UserId];

IF OBJECT_ID(N'[dbo].[FK_RefreshTokens_Users_UserId]', N'F') IS NOT NULL
    ALTER TABLE [dbo].[RefreshTokens] DROP CONSTRAINT [FK_RefreshTokens_Users_UserId];

IF OBJECT_ID(N'[dbo].[FK_WishlistItems_Users_UserId]', N'F') IS NOT NULL
    ALTER TABLE [dbo].[WishlistItems] DROP CONSTRAINT [FK_WishlistItems_Users_UserId];

IF OBJECT_ID(N'[dbo].[FK_PlanItems_Users_UserId]', N'F') IS NOT NULL
    ALTER TABLE [dbo].[PlanItems] DROP CONSTRAINT [FK_PlanItems_Users_UserId];

CREATE TABLE [dbo].[Users_Reordered]
(
{{columnDefinitions}},
    CONSTRAINT [PK_Users_Reordered] PRIMARY KEY CLUSTERED ([Id] ASC)
);

SET IDENTITY_INSERT [dbo].[Users_Reordered] ON;

INSERT INTO [dbo].[Users_Reordered] ({{orderedColumns}})
SELECT {{orderedColumns}}
FROM [dbo].[Users];

SET IDENTITY_INSERT [dbo].[Users_Reordered] OFF;

DROP TABLE [dbo].[Users];

EXEC sp_rename N'[dbo].[Users_Reordered]', N'Users';

ALTER TABLE [dbo].[Users] DROP CONSTRAINT [PK_Users_Reordered];
ALTER TABLE [dbo].[Users] ADD CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC);

CREATE UNIQUE INDEX [IX_Users_Email] ON [dbo].[Users] ([Email]);

IF OBJECT_ID(N'[dbo].[PasswordResetTokens]', N'U') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[PasswordResetTokens] WITH CHECK
        ADD CONSTRAINT [FK_PasswordResetTokens_Users_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[PasswordResetTokens] CHECK CONSTRAINT [FK_PasswordResetTokens_Users_UserId];
END;

IF OBJECT_ID(N'[dbo].[RefreshTokens]', N'U') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[RefreshTokens] WITH CHECK
        ADD CONSTRAINT [FK_RefreshTokens_Users_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[RefreshTokens] CHECK CONSTRAINT [FK_RefreshTokens_Users_UserId];
END;

IF OBJECT_ID(N'[dbo].[WishlistItems]', N'U') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[WishlistItems] WITH CHECK
        ADD CONSTRAINT [FK_WishlistItems_Users_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[WishlistItems] CHECK CONSTRAINT [FK_WishlistItems_Users_UserId];
END;

IF OBJECT_ID(N'[dbo].[PlanItems]', N'U') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[PlanItems] WITH CHECK
        ADD CONSTRAINT [FK_PlanItems_Users_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[PlanItems] CHECK CONSTRAINT [FK_PlanItems_Users_UserId];
END;
""";
        }
    }
}
