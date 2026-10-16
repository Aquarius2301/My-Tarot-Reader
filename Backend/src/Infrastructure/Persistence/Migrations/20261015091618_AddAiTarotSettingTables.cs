using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MyTarotReader.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAiTarotSettingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"AIChatMessages\";");

            migrationBuilder.Sql("DROP TABLE IF EXISTS \"AIChatTarotReadings\";");

            migrationBuilder.AlterColumn<string>(
                name: "QuestionType",
                table: "AITarotReadings",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            // Legacy question types were stored as the enum member name (e.g. "Energy"); the
            // column now holds the stable lowercase code consumed by AiTarotReadingType.Code.
            migrationBuilder.Sql(
                "UPDATE \"AITarotReadings\" SET \"QuestionType\" = lower(\"QuestionType\");"
            );

            // Legacy card counts were stored as the enum member name (e.g. "Three"); the column
            // is now an integer, so map the names while altering the column type. Guarded so the
            // migration is safe to re-run if a previous attempt partially applied.
            migrationBuilder.Sql(
                """
                DO $mig$
                DECLARE
                    col_type text;
                    sample text;
                BEGIN
                    SELECT data_type INTO col_type
                    FROM information_schema.columns
                    WHERE table_name = 'AITarotReadings' AND column_name = 'CardCount';

                    RAISE NOTICE 'AITarotReadings.CardCount current data_type = %', col_type;

                    IF col_type IN ('character varying', 'text') THEN
                        FOR sample IN EXECUTE 'SELECT DISTINCT "CardCount"::text FROM "AITarotReadings"' LOOP
                            RAISE NOTICE '  legacy CardCount value = %', sample;
                        END LOOP;

                        EXECUTE $sql$
                            ALTER TABLE "AITarotReadings"
                            ALTER COLUMN "CardCount" TYPE integer
                            USING (
                                CASE
                                    WHEN "CardCount" ~ '^[0-9]+$' THEN "CardCount"::integer
                                    WHEN lower("CardCount") = 'three' THEN 3
                                    WHEN lower("CardCount") = 'five' THEN 5
                                    WHEN lower("CardCount") = 'seven' THEN 7
                                    WHEN lower("CardCount") = 'ten' THEN 10
                                    ELSE 0
                                END
                            );
                        $sql$;
                    END IF;
                END
                $mig$;
                """
            );

            migrationBuilder.CreateTable(
                name: "AITarotReadingCounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CardCount = table.Column<int>(type: "integer", nullable: false),
                    WhiteCoins = table.Column<int>(type: "integer", nullable: false),
                    PositionsEn = table.Column<List<string>>(type: "text[]", nullable: false),
                    PositionsVi = table.Column<List<string>>(type: "text[]", nullable: false),
                    MinWords = table.Column<int>(type: "integer", nullable: false),
                    MaxWords = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AITarotReadingCounts", x => x.Id);
                    table.CheckConstraint("CK_AITarotReadingCounts_MinWords_Positive", "\"MinWords\" > 0");
                    table.CheckConstraint("CK_AITarotReadingCounts_WhiteCoins_NonNegative", "\"WhiteCoins\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "AITarotReadingTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameVi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AITarotReadingTypes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AITarotReadingCounts",
                columns: new[] { "Id", "CardCount", "CreatedAt", "DeletedAt", "IsEnabled", "MaxWords", "MinWords", "PositionsEn", "PositionsVi", "WhiteCoins" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), 3, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, 400, 300, new List<string> { "Core energy", "Challenges/obstacles", "Outcome/advice" }, new List<string> { "Năng lượng cốt lõi", "Thử thách/trở ngại", "Kết quả/lời khuyên" }, 2 },
                    { new Guid("22222222-2222-2222-2222-222222222222"), 5, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, 600, 450, new List<string> { "Core energy", "Challenges/obstacles", "Your strength", "Future", "Outcome/advice" }, new List<string> { "Năng lượng cốt lõi", "Thử thách/trở ngại", "Điểm mạnh của bạn", "Tương lai", "Kết quả/lời khuyên" }, 3 },
                    { new Guid("33333333-3333-3333-3333-333333333333"), 7, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, 800, 650, new List<string> { "Core energy", "Challenges/obstacles", "Your strength", "Hidden influences", "The way to face it", "Future", "Outcome/advice" }, new List<string> { "Năng lượng cốt lõi", "Thử thách/trở ngại", "Điểm mạnh của bạn", "Ảnh hưởng ẩn", "Cách đối mặt", "Tương lai", "Kết quả/lời khuyên" }, 4 },
                    { new Guid("44444444-4444-4444-4444-444444444444"), 10, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, 1000, 800, new List<string> { "Core energy", "Challenges/obstacles", "What to focus on", "Past", "Your strength", "Near future", "Suggested approach", "What you need to know", "Hopes/fears", "Outcome/advice" }, new List<string> { "Năng lượng cốt lõi", "Thử thách/trở ngại", "Điều cần tập trung", "Quá khứ", "Điểm mạnh của bạn", "Tương lai gần", "Hướng tiếp cận", "Điều bạn cần biết", "Hy vọng/nỗi sợ", "Kết quả/lời khuyên" }, 5 }
                });

            migrationBuilder.InsertData(
                table: "AITarotReadingTypes",
                columns: new[] { "Id", "Code", "CreatedAt", "DeletedAt", "IsEnabled", "NameEn", "NameVi" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000001"), "energy", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, "Energy", "Năng lượng" },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000002"), "love", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, "Love", "Tình yêu" },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000003"), "career", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, "Career", "Sự nghiệp" },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000004"), "money", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, "Money", "Tài chính" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AITarotReadingCounts_CardCount",
                table: "AITarotReadingCounts",
                column: "CardCount",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AITarotReadingTypes_Code",
                table: "AITarotReadingTypes",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AITarotReadingCounts");

            migrationBuilder.DropTable(
                name: "AITarotReadingTypes");

            migrationBuilder.AlterColumn<string>(
                name: "QuestionType",
                table: "AITarotReadings",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.Sql(
                "UPDATE \"AITarotReadings\" SET \"QuestionType\" = initcap(\"QuestionType\");"
            );

            migrationBuilder.Sql(
                """
                ALTER TABLE "AITarotReadings"
                ALTER COLUMN "CardCount" TYPE character varying(10)
                USING (
                    CASE
                        WHEN "CardCount" = 3 THEN 'Three'
                        WHEN "CardCount" = 5 THEN 'Five'
                        WHEN "CardCount" = 7 THEN 'Seven'
                        WHEN "CardCount" = 10 THEN 'Ten'
                        ELSE "CardCount"::text
                    END
                );
                """
            );

            migrationBuilder.CreateTable(
                name: "AIChatTarotReadings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PhaseMessageCount = table.Column<int>(type: "integer", nullable: false),
                    ReadingCount = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TotalCoinSpent = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIChatTarotReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIChatTarotReadings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AIChatMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatId = table.Column<Guid>(type: "uuid", nullable: false),
                    Cards = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Role = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIChatMessages_AIChatTarotReadings_ChatId",
                        column: x => x.ChatId,
                        principalTable: "AIChatTarotReadings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIChatMessages_ChatId",
                table: "AIChatMessages",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_AIChatTarotReadings_UserId",
                table: "AIChatTarotReadings",
                column: "UserId");
        }
    }
}
