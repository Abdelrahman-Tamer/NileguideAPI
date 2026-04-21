using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NileGuideApi.Migrations
{
    /// <inheritdoc />
    public partial class MoveActivityAuditColumnsToEnd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            MoveAuditColumnsToEnd(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            MoveAuditColumnsToEnd(migrationBuilder);
        }

        private static void MoveAuditColumnsToEnd(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE [Activities]
                ADD
                    [__CreatedAt] datetime NULL,
                    [__UpdatedAt] datetime NULL,
                    [__DeletedAt] datetime NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE [Activities]
                SET
                    [__CreatedAt] = [CreatedAt],
                    [__UpdatedAt] = [UpdatedAt],
                    [__DeletedAt] = [DeletedAt];
                """);

            migrationBuilder.Sql("""
                DECLARE @dropDefaultsSql nvarchar(max) = N'';

                SELECT @dropDefaultsSql +=
                    N'ALTER TABLE [Activities] DROP CONSTRAINT [' + dc.[name] + N'];'
                FROM sys.default_constraints dc
                INNER JOIN sys.columns c
                    ON c.[object_id] = dc.parent_object_id
                    AND c.column_id = dc.parent_column_id
                INNER JOIN sys.tables t
                    ON t.[object_id] = dc.parent_object_id
                WHERE t.[name] = N'Activities'
                    AND c.[name] IN (N'CreatedAt', N'UpdatedAt', N'DeletedAt');

                IF @dropDefaultsSql <> N''
                BEGIN
                    EXEC sp_executesql @dropDefaultsSql;
                END;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE [Activities]
                DROP COLUMN [CreatedAt], [UpdatedAt], [DeletedAt];
                """);

            migrationBuilder.Sql("""
                ALTER TABLE [Activities]
                ADD
                    [CreatedAt] datetime NULL,
                    [UpdatedAt] datetime NULL,
                    [DeletedAt] datetime NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE [Activities]
                SET
                    [CreatedAt] = COALESCE([__CreatedAt], GETDATE()),
                    [UpdatedAt] = COALESCE([__UpdatedAt], GETDATE()),
                    [DeletedAt] = [__DeletedAt];
                """);

            migrationBuilder.Sql("""
                ALTER TABLE [Activities]
                ALTER COLUMN [CreatedAt] datetime NOT NULL;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE [Activities]
                ALTER COLUMN [UpdatedAt] datetime NOT NULL;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE [Activities]
                ADD CONSTRAINT [DF_Activities_CreatedAt] DEFAULT (GETDATE()) FOR [CreatedAt];
                """);

            migrationBuilder.Sql("""
                ALTER TABLE [Activities]
                DROP COLUMN [__CreatedAt], [__UpdatedAt], [__DeletedAt];
                """);
        }
    }
}
