using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NileGuideApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfilePictureAndNumericActivityHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
IF COL_LENGTH(N'dbo.Users', N'profile_picture_url') IS NULL
    ALTER TABLE [dbo].[Users] ADD [profile_picture_url] varchar(500) NULL;

IF OBJECT_ID(N'[dbo].[ActivityHours]', N'U') IS NOT NULL
BEGIN
    IF OBJECT_ID(N'[dbo].[CK_ActivityHours_ClosingPeriod]', N'C') IS NOT NULL
        ALTER TABLE [dbo].[ActivityHours] DROP CONSTRAINT [CK_ActivityHours_ClosingPeriod];

    IF OBJECT_ID(N'[dbo].[CK_ActivityHours_OpeningPeriod]', N'C') IS NOT NULL
        ALTER TABLE [dbo].[ActivityHours] DROP CONSTRAINT [CK_ActivityHours_OpeningPeriod];

    IF OBJECT_ID(N'[dbo].[CK_ActivityHours_closing_period]', N'C') IS NOT NULL
        ALTER TABLE [dbo].[ActivityHours] DROP CONSTRAINT [CK_ActivityHours_closing_period];

    IF OBJECT_ID(N'[dbo].[CK_ActivityHours_opening_period]', N'C') IS NOT NULL
        ALTER TABLE [dbo].[ActivityHours] DROP CONSTRAINT [CK_ActivityHours_opening_period];

    IF OBJECT_ID(N'[dbo].[CK_ActivityHours_CloseAmPm]', N'C') IS NOT NULL
        ALTER TABLE [dbo].[ActivityHours] DROP CONSTRAINT [CK_ActivityHours_CloseAmPm];

    IF OBJECT_ID(N'[dbo].[CK_ActivityHours_CloseHour]', N'C') IS NOT NULL
        ALTER TABLE [dbo].[ActivityHours] DROP CONSTRAINT [CK_ActivityHours_CloseHour];

    IF OBJECT_ID(N'[dbo].[CK_ActivityHours_OpenAmPm]', N'C') IS NOT NULL
        ALTER TABLE [dbo].[ActivityHours] DROP CONSTRAINT [CK_ActivityHours_OpenAmPm];

    IF OBJECT_ID(N'[dbo].[CK_ActivityHours_OpenHour]', N'C') IS NOT NULL
        ALTER TABLE [dbo].[ActivityHours] DROP CONSTRAINT [CK_ActivityHours_OpenHour];

    IF COL_LENGTH(N'dbo.ActivityHours', N'OpeningHour') IS NOT NULL AND COL_LENGTH(N'dbo.ActivityHours', N'open_hour') IS NULL
        EXEC sp_rename N'dbo.ActivityHours.OpeningHour', N'open_hour', N'COLUMN';

    IF COL_LENGTH(N'dbo.ActivityHours', N'opening_hour') IS NOT NULL AND COL_LENGTH(N'dbo.ActivityHours', N'open_hour') IS NULL
        EXEC sp_rename N'dbo.ActivityHours.opening_hour', N'open_hour', N'COLUMN';

    IF COL_LENGTH(N'dbo.ActivityHours', N'OpeningPeriod') IS NOT NULL AND COL_LENGTH(N'dbo.ActivityHours', N'open_ampm') IS NULL
        EXEC sp_rename N'dbo.ActivityHours.OpeningPeriod', N'open_ampm', N'COLUMN';

    IF COL_LENGTH(N'dbo.ActivityHours', N'opening_period') IS NOT NULL AND COL_LENGTH(N'dbo.ActivityHours', N'open_ampm') IS NULL
        EXEC sp_rename N'dbo.ActivityHours.opening_period', N'open_ampm', N'COLUMN';

    IF COL_LENGTH(N'dbo.ActivityHours', N'ClosingHour') IS NOT NULL AND COL_LENGTH(N'dbo.ActivityHours', N'close_hour') IS NULL
        EXEC sp_rename N'dbo.ActivityHours.ClosingHour', N'close_hour', N'COLUMN';

    IF COL_LENGTH(N'dbo.ActivityHours', N'closing_hour') IS NOT NULL AND COL_LENGTH(N'dbo.ActivityHours', N'close_hour') IS NULL
        EXEC sp_rename N'dbo.ActivityHours.closing_hour', N'close_hour', N'COLUMN';

    IF COL_LENGTH(N'dbo.ActivityHours', N'ClosingPeriod') IS NOT NULL AND COL_LENGTH(N'dbo.ActivityHours', N'close_ampm') IS NULL
        EXEC sp_rename N'dbo.ActivityHours.ClosingPeriod', N'close_ampm', N'COLUMN';

    IF COL_LENGTH(N'dbo.ActivityHours', N'closing_period') IS NOT NULL AND COL_LENGTH(N'dbo.ActivityHours', N'close_ampm') IS NULL
        EXEC sp_rename N'dbo.ActivityHours.closing_period', N'close_ampm', N'COLUMN';

    IF COL_LENGTH(N'dbo.ActivityHours', N'open_hour') IS NULL
        ALTER TABLE [dbo].[ActivityHours] ADD [open_hour] tinyint NULL;

    IF COL_LENGTH(N'dbo.ActivityHours', N'open_ampm') IS NULL
        ALTER TABLE [dbo].[ActivityHours] ADD [open_ampm] varchar(2) NULL;

    IF COL_LENGTH(N'dbo.ActivityHours', N'close_hour') IS NULL
        ALTER TABLE [dbo].[ActivityHours] ADD [close_hour] tinyint NULL;

    IF COL_LENGTH(N'dbo.ActivityHours', N'close_ampm') IS NULL
        ALTER TABLE [dbo].[ActivityHours] ADD [close_ampm] varchar(2) NULL;

    EXEC(N'
        UPDATE [dbo].[ActivityHours]
        SET [open_hour] = CASE
                WHEN TRY_CONVERT(tinyint, [open_hour]) BETWEEN 1 AND 12 THEN TRY_CONVERT(tinyint, [open_hour])
                ELSE 1
            END,
            [close_hour] = CASE
                WHEN TRY_CONVERT(tinyint, [close_hour]) BETWEEN 1 AND 12 THEN TRY_CONVERT(tinyint, [close_hour])
                ELSE 12
            END,
            [open_ampm] = CASE
                WHEN UPPER(LTRIM(RTRIM(CONVERT(varchar(10), [open_ampm])))) IN (''AM'', ''PM'') THEN UPPER(LTRIM(RTRIM(CONVERT(varchar(10), [open_ampm]))))
                ELSE ''AM''
            END,
            [close_ampm] = CASE
                WHEN UPPER(LTRIM(RTRIM(CONVERT(varchar(10), [close_ampm])))) IN (''AM'', ''PM'') THEN UPPER(LTRIM(RTRIM(CONVERT(varchar(10), [close_ampm]))))
                ELSE ''PM''
            END;

        ALTER TABLE [dbo].[ActivityHours] ALTER COLUMN [open_hour] tinyint NOT NULL;
        ALTER TABLE [dbo].[ActivityHours] ALTER COLUMN [close_hour] tinyint NOT NULL;
        ALTER TABLE [dbo].[ActivityHours] ALTER COLUMN [open_ampm] varchar(2) NOT NULL;
        ALTER TABLE [dbo].[ActivityHours] ALTER COLUMN [close_ampm] varchar(2) NOT NULL;

        ALTER TABLE [dbo].[ActivityHours] ADD CONSTRAINT [CK_ActivityHours_OpenHour] CHECK ([open_hour] BETWEEN 1 AND 12);
        ALTER TABLE [dbo].[ActivityHours] ADD CONSTRAINT [CK_ActivityHours_CloseHour] CHECK ([close_hour] BETWEEN 1 AND 12);
        ALTER TABLE [dbo].[ActivityHours] ADD CONSTRAINT [CK_ActivityHours_OpenAmPm] CHECK ([open_ampm] IN (''AM'', ''PM''));
        ALTER TABLE [dbo].[ActivityHours] ADD CONSTRAINT [CK_ActivityHours_CloseAmPm] CHECK ([close_ampm] IN (''AM'', ''PM''));
    ');
END
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
IF COL_LENGTH(N'dbo.Users', N'profile_picture_url') IS NOT NULL
    ALTER TABLE [dbo].[Users] DROP COLUMN [profile_picture_url];

IF OBJECT_ID(N'[dbo].[ActivityHours]', N'U') IS NOT NULL
BEGIN
    IF OBJECT_ID(N'[dbo].[CK_ActivityHours_CloseAmPm]', N'C') IS NOT NULL
        ALTER TABLE [dbo].[ActivityHours] DROP CONSTRAINT [CK_ActivityHours_CloseAmPm];

    IF OBJECT_ID(N'[dbo].[CK_ActivityHours_CloseHour]', N'C') IS NOT NULL
        ALTER TABLE [dbo].[ActivityHours] DROP CONSTRAINT [CK_ActivityHours_CloseHour];

    IF OBJECT_ID(N'[dbo].[CK_ActivityHours_OpenAmPm]', N'C') IS NOT NULL
        ALTER TABLE [dbo].[ActivityHours] DROP CONSTRAINT [CK_ActivityHours_OpenAmPm];

    IF OBJECT_ID(N'[dbo].[CK_ActivityHours_OpenHour]', N'C') IS NOT NULL
        ALTER TABLE [dbo].[ActivityHours] DROP CONSTRAINT [CK_ActivityHours_OpenHour];

    EXEC(N'
        ALTER TABLE [dbo].[ActivityHours] ALTER COLUMN [open_hour] tinyint NULL;
        ALTER TABLE [dbo].[ActivityHours] ALTER COLUMN [close_hour] tinyint NULL;
        ALTER TABLE [dbo].[ActivityHours] ALTER COLUMN [open_ampm] varchar(2) NULL;
        ALTER TABLE [dbo].[ActivityHours] ALTER COLUMN [close_ampm] varchar(2) NULL;

        UPDATE [dbo].[ActivityHours]
        SET [open_ampm] = LOWER([open_ampm]),
            [close_ampm] = LOWER([close_ampm]);
    ');

    IF COL_LENGTH(N'dbo.ActivityHours', N'open_hour') IS NOT NULL AND COL_LENGTH(N'dbo.ActivityHours', N'OpeningHour') IS NULL
        EXEC sp_rename N'dbo.ActivityHours.open_hour', N'OpeningHour', N'COLUMN';

    IF COL_LENGTH(N'dbo.ActivityHours', N'open_ampm') IS NOT NULL AND COL_LENGTH(N'dbo.ActivityHours', N'OpeningPeriod') IS NULL
        EXEC sp_rename N'dbo.ActivityHours.open_ampm', N'OpeningPeriod', N'COLUMN';

    IF COL_LENGTH(N'dbo.ActivityHours', N'close_hour') IS NOT NULL AND COL_LENGTH(N'dbo.ActivityHours', N'ClosingHour') IS NULL
        EXEC sp_rename N'dbo.ActivityHours.close_hour', N'ClosingHour', N'COLUMN';

    IF COL_LENGTH(N'dbo.ActivityHours', N'close_ampm') IS NOT NULL AND COL_LENGTH(N'dbo.ActivityHours', N'ClosingPeriod') IS NULL
        EXEC sp_rename N'dbo.ActivityHours.close_ampm', N'ClosingPeriod', N'COLUMN';

    IF OBJECT_ID(N'[dbo].[CK_ActivityHours_ClosingPeriod]', N'C') IS NULL
        EXEC(N'ALTER TABLE [dbo].[ActivityHours] ADD CONSTRAINT [CK_ActivityHours_ClosingPeriod] CHECK ([ClosingPeriod] IS NULL OR [ClosingPeriod] IN (''am'', ''pm''));');

    IF OBJECT_ID(N'[dbo].[CK_ActivityHours_OpeningPeriod]', N'C') IS NULL
        EXEC(N'ALTER TABLE [dbo].[ActivityHours] ADD CONSTRAINT [CK_ActivityHours_OpeningPeriod] CHECK ([OpeningPeriod] IS NULL OR [OpeningPeriod] IN (''am'', ''pm''));');
END
""");
        }
    }
}
