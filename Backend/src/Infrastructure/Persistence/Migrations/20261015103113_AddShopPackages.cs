using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MyTarotReader.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShopPackages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WhiteCoins",
                table: "PaymentOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.InsertData(
                table: "ShopPackages",
                columns: new[] { "Id", "Code", "CreatedAt", "DeletedAt", "IsEnabled", "OldPrice", "Price", "RedCoins", "WhiteCoins" },
                values: new object[,]
                {
                    { new Guid("55555555-5555-5555-5555-555555555551"), "pack-10", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, 10000, 10000, 10, 0 },
                    { new Guid("55555555-5555-5555-5555-555555555552"), "pack-30", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, 27000, 27000, 30, 0 },
                    { new Guid("55555555-5555-5555-5555-555555555553"), "pack-50", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, 40000, 40000, 50, 0 }
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_PaymentOrders_WhiteCoins_NonNegative",
                table: "PaymentOrders",
                sql: "\"WhiteCoins\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_ShopPackages_Code",
                table: "ShopPackages",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShopPackages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PaymentOrders_WhiteCoins_NonNegative",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "WhiteCoins",
                table: "PaymentOrders");
        }
    }
}
