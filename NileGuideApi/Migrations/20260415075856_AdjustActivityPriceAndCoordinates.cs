using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NileGuideApi.Migrations
{
    /// <inheritdoc />
    public partial class AdjustActivityPriceAndCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PriceMinEst",
                table: "Activities",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "PriceMaxEst",
                table: "Activities",
                newName: "MinPrice");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Activities",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 22)
                .OldAnnotation("Relational:ColumnOrder", 20);

            migrationBuilder.AlterColumn<int>(
                name: "ReviewCount",
                table: "Activities",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 18)
                .OldAnnotation("Relational:ColumnOrder", 16);

            migrationBuilder.AlterColumn<double>(
                name: "Rating",
                table: "Activities",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float")
                .Annotation("Relational:ColumnOrder", 17)
                .OldAnnotation("Relational:ColumnOrder", 15);

            migrationBuilder.AlterColumn<string>(
                name: "Provider",
                table: "Activities",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 20)
                .OldAnnotation("Relational:ColumnOrder", 18);

            migrationBuilder.AlterColumn<string>(
                name: "ExternalId",
                table: "Activities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 19)
                .OldAnnotation("Relational:ColumnOrder", 17);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                table: "Activities",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 23)
                .OldAnnotation("Relational:ColumnOrder", 21);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Activities",
                type: "datetime",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "GETDATE()")
                .Annotation("Relational:ColumnOrder", 21)
                .OldAnnotation("Relational:ColumnOrder", 19);

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Activities",
                type: "decimal(10,7)",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 15);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Activities",
                type: "decimal(10,7)",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 16);

            MoveAuditColumnsToEnd(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Activities");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Activities",
                newName: "PriceMinEst");

            migrationBuilder.RenameColumn(
                name: "MinPrice",
                table: "Activities",
                newName: "PriceMaxEst");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Activities",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 20)
                .OldAnnotation("Relational:ColumnOrder", 22);

            migrationBuilder.AlterColumn<int>(
                name: "ReviewCount",
                table: "Activities",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 16)
                .OldAnnotation("Relational:ColumnOrder", 18);

            migrationBuilder.AlterColumn<double>(
                name: "Rating",
                table: "Activities",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float")
                .Annotation("Relational:ColumnOrder", 15)
                .OldAnnotation("Relational:ColumnOrder", 17);

            migrationBuilder.AlterColumn<string>(
                name: "Provider",
                table: "Activities",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 18)
                .OldAnnotation("Relational:ColumnOrder", 20);

            migrationBuilder.AlterColumn<string>(
                name: "ExternalId",
                table: "Activities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 17)
                .OldAnnotation("Relational:ColumnOrder", 19);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                table: "Activities",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 21)
                .OldAnnotation("Relational:ColumnOrder", 23);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Activities",
                type: "datetime",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "GETDATE()")
                .Annotation("Relational:ColumnOrder", 19)
                .OldAnnotation("Relational:ColumnOrder", 21);
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
