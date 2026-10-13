using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace JourneyTracker.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Journeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TotalDistanceKm = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Journeys", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    JourneyId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DistanceKm = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Activities_Journeys_JourneyId",
                        column: x => x.JourneyId,
                        principalTable: "Journeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Stops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    JourneyId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DistanceFromStartKm = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stops_Journeys_JourneyId",
                        column: x => x.JourneyId,
                        principalTable: "Journeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Journeys",
                columns: new[] { "Id", "Name", "TotalDistanceKm" },
                values: new object[,]
                {
                    { 1, "Hanoi to Ho Chi Minh City", 1700m },
                    { 2, "Tokyo to Osaka", 510m },
                    { 3, "Melbourne to Sydney", 880m }
                });

            migrationBuilder.InsertData(
                table: "Stops",
                columns: new[] { "Id", "DistanceFromStartKm", "JourneyId", "Name" },
                values: new object[,]
                {
                    { 1, 0m, 1, "Hanoi" },
                    { 2, 95m, 1, "Ninh Binh" },
                    { 3, 295m, 1, "Vinh" },
                    { 4, 665m, 1, "Hue" },
                    { 5, 760m, 1, "Da Nang" },
                    { 6, 1065m, 1, "Quy Nhon" },
                    { 7, 1280m, 1, "Nha Trang" },
                    { 8, 1700m, 1, "Ho Chi Minh City" },
                    { 9, 0m, 2, "Tokyo" },
                    { 10, 30m, 2, "Yokohama" },
                    { 11, 180m, 2, "Shizuoka" },
                    { 12, 350m, 2, "Nagoya" },
                    { 13, 470m, 2, "Kyoto" },
                    { 14, 510m, 2, "Osaka" },
                    { 15, 0m, 3, "Melbourne" },
                    { 16, 100m, 3, "Seymour" },
                    { 17, 235m, 3, "Wangaratta" },
                    { 18, 325m, 3, "Albury" },
                    { 19, 500m, 3, "Gundagai" },
                    { 20, 690m, 3, "Goulburn" },
                    { 21, 880m, 3, "Sydney" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_JourneyId",
                table: "Activities",
                column: "JourneyId");

            migrationBuilder.CreateIndex(
                name: "IX_Stops_JourneyId",
                table: "Stops",
                column: "JourneyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Activities");

            migrationBuilder.DropTable(
                name: "Stops");

            migrationBuilder.DropTable(
                name: "Journeys");
        }
    }
}
