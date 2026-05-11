using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NileGuideApi.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityHoursTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityHours",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityID = table.Column<int>(type: "int", nullable: false),
                    OpeningHour = table.Column<byte>(type: "tinyint", nullable: true),
                    OpeningPeriod = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: true),
                    ClosingHour = table.Column<byte>(type: "tinyint", nullable: true),
                    ClosingPeriod = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityHours", x => x.id);
                    table.CheckConstraint("CK_ActivityHours_ClosingPeriod", "[ClosingPeriod] IS NULL OR [ClosingPeriod] IN ('am', 'pm')");
                    table.CheckConstraint("CK_ActivityHours_OpeningPeriod", "[OpeningPeriod] IS NULL OR [OpeningPeriod] IN ('am', 'pm')");
                    table.ForeignKey(
                        name: "FK_ActivityHours_Activities_ActivityID",
                        column: x => x.ActivityID,
                        principalTable: "Activities",
                        principalColumn: "ActivityID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityHours_ActivityID",
                table: "ActivityHours",
                column: "ActivityID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityHours");
        }
    }
}
