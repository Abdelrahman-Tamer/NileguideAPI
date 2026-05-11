using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NileGuideApi.Data;

#nullable disable

namespace NileGuideApi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260417001000_FixActivityHoursColumnNames")]
    public class FixActivityHoursColumnNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[dbo].[ActivityHours]', N'U') IS NOT NULL
                BEGIN
                    IF OBJECT_ID(N'[dbo].[FK_ActivityHours_Activities_activity_id]', N'F') IS NOT NULL
                        ALTER TABLE [dbo].[ActivityHours] DROP CONSTRAINT [FK_ActivityHours_Activities_activity_id];

                    IF OBJECT_ID(N'[dbo].[CK_ActivityHours_closing_period]', N'C') IS NOT NULL
                        ALTER TABLE [dbo].[ActivityHours] DROP CONSTRAINT [CK_ActivityHours_closing_period];

                    IF OBJECT_ID(N'[dbo].[CK_ActivityHours_opening_period]', N'C') IS NOT NULL
                        ALTER TABLE [dbo].[ActivityHours] DROP CONSTRAINT [CK_ActivityHours_opening_period];
                END;
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.ActivityHours', N'activity_id') IS NOT NULL
                    EXEC sp_rename N'dbo.ActivityHours.activity_id', N'ActivityID', N'COLUMN';
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.ActivityHours', N'opening_hour') IS NOT NULL
                    EXEC sp_rename N'dbo.ActivityHours.opening_hour', N'OpeningHour', N'COLUMN';
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.ActivityHours', N'opening_period') IS NOT NULL
                    EXEC sp_rename N'dbo.ActivityHours.opening_period', N'OpeningPeriod', N'COLUMN';
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.ActivityHours', N'closing_hour') IS NOT NULL
                    EXEC sp_rename N'dbo.ActivityHours.closing_hour', N'ClosingHour', N'COLUMN';
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.ActivityHours', N'closing_period') IS NOT NULL
                    EXEC sp_rename N'dbo.ActivityHours.closing_period', N'ClosingPeriod', N'COLUMN';
                """);

            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE object_id = OBJECT_ID(N'[dbo].[ActivityHours]')
                      AND name = N'IX_ActivityHours_activity_id'
                )
                    EXEC sp_rename N'dbo.ActivityHours.IX_ActivityHours_activity_id', N'IX_ActivityHours_ActivityID', N'INDEX';
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[dbo].[CK_ActivityHours_ClosingPeriod]', N'C') IS NULL
                    ALTER TABLE [dbo].[ActivityHours]
                    ADD CONSTRAINT [CK_ActivityHours_ClosingPeriod]
                    CHECK ([ClosingPeriod] IS NULL OR [ClosingPeriod] IN ('am', 'pm'));
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[dbo].[CK_ActivityHours_OpeningPeriod]', N'C') IS NULL
                    ALTER TABLE [dbo].[ActivityHours]
                    ADD CONSTRAINT [CK_ActivityHours_OpeningPeriod]
                    CHECK ([OpeningPeriod] IS NULL OR [OpeningPeriod] IN ('am', 'pm'));
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[dbo].[FK_ActivityHours_Activities_ActivityID]', N'F') IS NULL
                    ALTER TABLE [dbo].[ActivityHours]
                    ADD CONSTRAINT [FK_ActivityHours_Activities_ActivityID]
                    FOREIGN KEY ([ActivityID]) REFERENCES [dbo].[Activities]([ActivityID]) ON DELETE CASCADE;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[dbo].[ActivityHours]', N'U') IS NOT NULL
                BEGIN
                    IF OBJECT_ID(N'[dbo].[FK_ActivityHours_Activities_ActivityID]', N'F') IS NOT NULL
                        ALTER TABLE [dbo].[ActivityHours] DROP CONSTRAINT [FK_ActivityHours_Activities_ActivityID];

                    IF OBJECT_ID(N'[dbo].[CK_ActivityHours_ClosingPeriod]', N'C') IS NOT NULL
                        ALTER TABLE [dbo].[ActivityHours] DROP CONSTRAINT [CK_ActivityHours_ClosingPeriod];

                    IF OBJECT_ID(N'[dbo].[CK_ActivityHours_OpeningPeriod]', N'C') IS NOT NULL
                        ALTER TABLE [dbo].[ActivityHours] DROP CONSTRAINT [CK_ActivityHours_OpeningPeriod];
                END;
                """);

            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE object_id = OBJECT_ID(N'[dbo].[ActivityHours]')
                      AND name = N'IX_ActivityHours_ActivityID'
                )
                    EXEC sp_rename N'dbo.ActivityHours.IX_ActivityHours_ActivityID', N'IX_ActivityHours_activity_id', N'INDEX';
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.ActivityHours', N'ActivityID') IS NOT NULL
                    EXEC sp_rename N'dbo.ActivityHours.ActivityID', N'activity_id', N'COLUMN';
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.ActivityHours', N'OpeningHour') IS NOT NULL
                    EXEC sp_rename N'dbo.ActivityHours.OpeningHour', N'opening_hour', N'COLUMN';
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.ActivityHours', N'OpeningPeriod') IS NOT NULL
                    EXEC sp_rename N'dbo.ActivityHours.OpeningPeriod', N'opening_period', N'COLUMN';
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.ActivityHours', N'ClosingHour') IS NOT NULL
                    EXEC sp_rename N'dbo.ActivityHours.ClosingHour', N'closing_hour', N'COLUMN';
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.ActivityHours', N'ClosingPeriod') IS NOT NULL
                    EXEC sp_rename N'dbo.ActivityHours.ClosingPeriod', N'closing_period', N'COLUMN';
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[dbo].[CK_ActivityHours_closing_period]', N'C') IS NULL
                    ALTER TABLE [dbo].[ActivityHours]
                    ADD CONSTRAINT [CK_ActivityHours_closing_period]
                    CHECK ([closing_period] IS NULL OR [closing_period] IN ('am', 'pm'));
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[dbo].[CK_ActivityHours_opening_period]', N'C') IS NULL
                    ALTER TABLE [dbo].[ActivityHours]
                    ADD CONSTRAINT [CK_ActivityHours_opening_period]
                    CHECK ([opening_period] IS NULL OR [opening_period] IN ('am', 'pm'));
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[dbo].[FK_ActivityHours_Activities_activity_id]', N'F') IS NULL
                    ALTER TABLE [dbo].[ActivityHours]
                    ADD CONSTRAINT [FK_ActivityHours_Activities_activity_id]
                    FOREIGN KEY ([activity_id]) REFERENCES [dbo].[Activities]([ActivityID]) ON DELETE CASCADE;
                """);
        }
    }
}
