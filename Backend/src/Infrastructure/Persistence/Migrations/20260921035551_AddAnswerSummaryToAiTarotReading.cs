using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyTarotReader.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAnswerSummaryToAiTarotReading : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnswerSummary",
                table: "AITarotReadings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "AITarotReadings",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnswerSummary",
                table: "AITarotReadings");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "AITarotReadings");
        }
    }
}
