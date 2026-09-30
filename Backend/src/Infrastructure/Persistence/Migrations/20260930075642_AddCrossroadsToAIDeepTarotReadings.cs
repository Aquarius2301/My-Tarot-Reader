using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyTarotReader.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCrossroadsToAIDeepTarotReadings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Options",
                table: "AIDeepTarotReadings",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Question",
                table: "AIDeepTarotReadings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeFrame",
                table: "AIDeepTarotReadings",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Options",
                table: "AIDeepTarotReadings");

            migrationBuilder.DropColumn(
                name: "Question",
                table: "AIDeepTarotReadings");

            migrationBuilder.DropColumn(
                name: "TimeFrame",
                table: "AIDeepTarotReadings");
        }
    }
}
