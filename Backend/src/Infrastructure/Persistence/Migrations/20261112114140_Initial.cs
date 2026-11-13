using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MyTarotReader.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(
                name: "ShopPackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RedCoins = table.Column<int>(type: "integer", nullable: false),
                    WhiteCoins = table.Column<int>(type: "integer", nullable: false),
                    OldPrice = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopPackages", x => x.Id);
                    table.CheckConstraint("CK_ShopPackages_OldPrice_NonNegative", "\"OldPrice\" >= 0");
                    table.CheckConstraint("CK_ShopPackages_Price_Positive", "\"Price\" > 0");
                    table.CheckConstraint("CK_ShopPackages_RedCoins_NonNegative", "\"RedCoins\" >= 0");
                    table.CheckConstraint("CK_ShopPackages_WhiteCoins_NonNegative", "\"WhiteCoins\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Picture = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AIDeepTarotReadings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Topic = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    AnswerSummary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Answer = table.Column<string>(type: "text", nullable: false),
                    Cards = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Question = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Options = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TimeFrame = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIDeepTarotReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIDeepTarotReadings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AITarotReadings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    CardCount = table.Column<int>(type: "integer", nullable: false),
                    QuestionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AnswerSummary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Answer = table.Column<string>(type: "text", nullable: false),
                    Cards = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AITarotReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AITarotReadings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RedCoins = table.Column<int>(type: "integer", nullable: false),
                    WhiteCoins = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderCode = table.Column<long>(type: "bigint", nullable: false),
                    PackageCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AmountVnd = table.Column<int>(type: "integer", nullable: false),
                    RedCoins = table.Column<int>(type: "integer", nullable: false),
                    WhiteCoins = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PayOsPaymentLinkId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CheckoutUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    QrCode = table.Column<string>(type: "text", nullable: true),
                    PayOsTransactionReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PaidAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentOrders", x => x.Id);
                    table.CheckConstraint("CK_PaymentOrders_AmountVnd_Positive", "\"AmountVnd\" > 0");
                    table.CheckConstraint("CK_PaymentOrders_RedCoins_Positive", "\"RedCoins\" > 0");
                    table.CheckConstraint("CK_PaymentOrders_WhiteCoins_NonNegative", "\"WhiteCoins\" >= 0");
                    table.ForeignKey(
                        name: "FK_PaymentOrders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeviceFingerprint = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Streaks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentStreak = table.Column<int>(type: "integer", nullable: false),
                    LongestStreak = table.Column<int>(type: "integer", nullable: false),
                    CycleDay = table.Column<int>(type: "integer", nullable: false),
                    LastCheckIn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsSaverUsed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Streaks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Streaks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TarotReadings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CardCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsReversed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TarotReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TarotReadings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RedCoin = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.Id);
                    table.CheckConstraint("CK_Wallets_RedCoin_NonNegative", "\"RedCoin\" >= 0");
                    table.ForeignKey(
                        name: "FK_Wallets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WhiteCoinBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    RemainingAmount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ExpiredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhiteCoinBatches", x => x.Id);
                    table.CheckConstraint("CK_WhiteCoinBatches_Amount_NonNegative", "\"Amount\" >= 0");
                    table.CheckConstraint("CK_WhiteCoinBatches_RemainingAmount_NonNegative", "\"RemainingAmount\" >= 0");
                    table.ForeignKey(
                        name: "FK_WhiteCoinBatches_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.InsertData(
                table: "ShopPackages",
                columns: new[] { "Id", "Code", "CreatedAt", "DeletedAt", "IsEnabled", "OldPrice", "Price", "RedCoins", "WhiteCoins" },
                values: new object[,]
                {
                    { new Guid("55555555-5555-5555-5555-555555555551"), "pack-10", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, 10000, 10000, 10, 0 },
                    { new Guid("55555555-5555-5555-5555-555555555552"), "pack-30", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, 27000, 27000, 30, 0 },
                    { new Guid("55555555-5555-5555-5555-555555555553"), "pack-50", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, 40000, 40000, 50, 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIDeepTarotReadings_UserId",
                table: "AIDeepTarotReadings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AITarotReadingCounts_CardCount",
                table: "AITarotReadingCounts",
                column: "CardCount",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AITarotReadings_UserId",
                table: "AITarotReadings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AITarotReadingTypes_Code",
                table: "AITarotReadingTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrders_OrderCode",
                table: "PaymentOrders",
                column: "OrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrders_UserId",
                table: "PaymentOrders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId_DeviceFingerprint",
                table: "RefreshTokens",
                columns: new[] { "UserId", "DeviceFingerprint" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopPackages_Code",
                table: "ShopPackages",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Streaks_UserId",
                table: "Streaks",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TarotReadings_UserId",
                table: "TarotReadings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ProviderKey",
                table: "Users",
                column: "ProviderKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_UserId",
                table: "Wallets",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WhiteCoinBatches_WalletId",
                table: "WhiteCoinBatches",
                column: "WalletId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIDeepTarotReadings");

            migrationBuilder.DropTable(
                name: "AITarotReadingCounts");

            migrationBuilder.DropTable(
                name: "AITarotReadings");

            migrationBuilder.DropTable(
                name: "AITarotReadingTypes");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "PaymentOrders");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "ShopPackages");

            migrationBuilder.DropTable(
                name: "Streaks");

            migrationBuilder.DropTable(
                name: "TarotReadings");

            migrationBuilder.DropTable(
                name: "WhiteCoinBatches");

            migrationBuilder.DropTable(
                name: "Wallets");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
