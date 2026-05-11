using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NileGuideApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialActivitiesSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Activities_PriceBasis_Allowed",
                table: "Activities");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Activities_Status_Allowed",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Activities");

            migrationBuilder.RenameColumn(
                name: "OpeningHours",
                table: "Activities",
                newName: "Duration");

            migrationBuilder.DropColumn(
                name: "RequiresPersonalID",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Activities");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "ActivityImages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<bool>(
                name: "IsPrimary",
                table: "ActivityImages",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ActivityImages",
                type: "datetime",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<double>(
                name: "Rating",
                table: "Activities",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(3,2)");

            migrationBuilder.AlterColumn<string>(
                name: "PriceCurrency",
                table: "Activities",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "USD",
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldMaxLength: 10,
                oldDefaultValue: "USD");

            migrationBuilder.AlterColumn<string>(
                name: "PriceBasis",
                table: "Activities",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Activities",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Activities",
                type: "datetime",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<string>(
                name: "Duration",
                table: "Activities",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ActivityName",
                table: "Activities",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "Cancellation",
                table: "Activities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Activities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GroupSize",
                table: "Activities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "Activities",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "Activities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequiredDocuments",
                table: "Activities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BookingLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityID = table.Column<int>(type: "int", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingLinks_Activities_ActivityID",
                        column: x => x.ActivityID,
                        principalTable: "Activities",
                        principalColumn: "ActivityID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingLinks_ActivityID",
                table: "BookingLinks",
                column: "ActivityID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingLinks");

            migrationBuilder.DropColumn(
                name: "Cancellation",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "GroupSize",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "RequiredDocuments",
                table: "Activities");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "ActivityImages",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPrimary",
                table: "ActivityImages",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ActivityImages",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<decimal>(
                name: "Rating",
                table: "Activities",
                type: "decimal(3,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<string>(
                name: "PriceCurrency",
                table: "Activities",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "USD",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldDefaultValue: "USD");

            migrationBuilder.AlterColumn<string>(
                name: "PriceBasis",
                table: "Activities",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Activities",
                type: "varchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Activities",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "Duration",
                table: "Activities",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ActivityName",
                table: "Activities",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Activities",
                type: "decimal(10,7)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Activities",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Activities",
                type: "decimal(10,7)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresPersonalID",
                table: "Activities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Activities",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Available");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Activities_PriceBasis_Allowed",
                table: "Activities",
                sql: "[PriceBasis] IS NULL OR [PriceBasis] IN ('Free', 'PerPerson', 'PerTicket', 'PerHour', 'PerTrip', 'PerNight')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Activities_Status_Allowed",
                table: "Activities",
                sql: "[Status] IN ('Available', 'Unavailable', 'Temporarily Closed')");

            migrationBuilder.RenameColumn(
                name: "Duration",
                table: "Activities",
                newName: "OpeningHours");
        }
    }
}
