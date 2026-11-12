using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyTarotReader.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentOrderCheckoutData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CheckoutUrl",
                table: "PaymentOrders",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QrCode",
                table: "PaymentOrders",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AITarotReadingCounts",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "PositionsEn", "PositionsVi" },
                values: new object[] { new List<string> { "Core energy", "Challenges/obstacles", "Outcome/advice" }, new List<string> { "Năng lượng cốt lõi", "Thử thách/trở ngại", "Kết quả/lời khuyên" } });

            migrationBuilder.UpdateData(
                table: "AITarotReadingCounts",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "PositionsEn", "PositionsVi" },
                values: new object[] { new List<string> { "Core energy", "Challenges/obstacles", "Your strength", "Future", "Outcome/advice" }, new List<string> { "Năng lượng cốt lõi", "Thử thách/trở ngại", "Điểm mạnh của bạn", "Tương lai", "Kết quả/lời khuyên" } });

            migrationBuilder.UpdateData(
                table: "AITarotReadingCounts",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "PositionsEn", "PositionsVi" },
                values: new object[] { new List<string> { "Core energy", "Challenges/obstacles", "Your strength", "Hidden influences", "The way to face it", "Future", "Outcome/advice" }, new List<string> { "Năng lượng cốt lõi", "Thử thách/trở ngại", "Điểm mạnh của bạn", "Ảnh hưởng ẩn", "Cách đối mặt", "Tương lai", "Kết quả/lời khuyên" } });

            migrationBuilder.UpdateData(
                table: "AITarotReadingCounts",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "PositionsEn", "PositionsVi" },
                values: new object[] { new List<string> { "Core energy", "Challenges/obstacles", "What to focus on", "Past", "Your strength", "Near future", "Suggested approach", "What you need to know", "Hopes/fears", "Outcome/advice" }, new List<string> { "Năng lượng cốt lõi", "Thử thách/trở ngại", "Điều cần tập trung", "Quá khứ", "Điểm mạnh của bạn", "Tương lai gần", "Hướng tiếp cận", "Điều bạn cần biết", "Hy vọng/nỗi sợ", "Kết quả/lời khuyên" } });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckoutUrl",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "QrCode",
                table: "PaymentOrders");

            migrationBuilder.UpdateData(
                table: "AITarotReadingCounts",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "PositionsEn", "PositionsVi" },
                values: new object[] { new List<string> { "Core energy", "Challenges/obstacles", "Outcome/advice" }, new List<string> { "Năng lượng cốt lõi", "Thử thách/trở ngại", "Kết quả/lời khuyên" } });

            migrationBuilder.UpdateData(
                table: "AITarotReadingCounts",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "PositionsEn", "PositionsVi" },
                values: new object[] { new List<string> { "Core energy", "Challenges/obstacles", "Your strength", "Future", "Outcome/advice" }, new List<string> { "Năng lượng cốt lõi", "Thử thách/trở ngại", "Điểm mạnh của bạn", "Tương lai", "Kết quả/lời khuyên" } });

            migrationBuilder.UpdateData(
                table: "AITarotReadingCounts",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "PositionsEn", "PositionsVi" },
                values: new object[] { new List<string> { "Core energy", "Challenges/obstacles", "Your strength", "Hidden influences", "The way to face it", "Future", "Outcome/advice" }, new List<string> { "Năng lượng cốt lõi", "Thử thách/trở ngại", "Điểm mạnh của bạn", "Ảnh hưởng ẩn", "Cách đối mặt", "Tương lai", "Kết quả/lời khuyên" } });

            migrationBuilder.UpdateData(
                table: "AITarotReadingCounts",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "PositionsEn", "PositionsVi" },
                values: new object[] { new List<string> { "Core energy", "Challenges/obstacles", "What to focus on", "Past", "Your strength", "Near future", "Suggested approach", "What you need to know", "Hopes/fears", "Outcome/advice" }, new List<string> { "Năng lượng cốt lõi", "Thử thách/trở ngại", "Điều cần tập trung", "Quá khứ", "Điểm mạnh của bạn", "Tương lai gần", "Hướng tiếp cận", "Điều bạn cần biết", "Hy vọng/nỗi sợ", "Kết quả/lời khuyên" } });
        }
    }
}
