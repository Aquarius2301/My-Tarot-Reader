using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyTarotReader.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderCoins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderDetails");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Orders",
                newName: "WhiteCoins");

            migrationBuilder.AddColumn<int>(
                name: "RedCoins",
                table: "Orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RedCoins",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "WhiteCoins",
                table: "Orders",
                newName: "Amount");

            migrationBuilder.CreateTable(
                name: "OrderDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    WhiteCoinBatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderDetails_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDetails_WhiteCoinBatches_WhiteCoinBatchId",
                        column: x => x.WhiteCoinBatchId,
                        principalTable: "WhiteCoinBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_OrderId",
                table: "OrderDetails",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_WhiteCoinBatchId",
                table: "OrderDetails",
                column: "WhiteCoinBatchId");
        }
    }
}
