using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Features.AiTarot;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.UnitTest.Common;
using Xunit;

namespace MyTarotReader.UnitTest.Features.AiTarot;

public class CreateAiTarotReadingCountCommandHandlerTests
{
    private static CreateAiTarotReadingCountCommandHandler CreateSut(AppDbContext db) => new(db);

    private static AITarotReadingCount SeedReadingCount(
        AppDbContext db,
        int cardCount,
        bool deleted = false,
        int whiteCoins = 2,
        int minWords = 100,
        int maxWords = 200,
        bool isEnabled = true
    )
    {
        var readingCount = new AITarotReadingCount
        {
            CardCount = cardCount,
            WhiteCoins = whiteCoins,
            PositionsEn = Enumerable
                .Range(1, cardCount)
                .Select(index => $"Position {index}")
                .ToList(),
            PositionsVi = Enumerable
                .Range(1, cardCount)
                .Select(index => $"Vị trí {index}")
                .ToList(),
            MinWords = minWords,
            MaxWords = maxWords,
            IsEnabled = isEnabled,
            CreatedAt = DateTimeOffset.UtcNow,
            DeletedAt = deleted ? DateTimeOffset.UtcNow : null,
        };
        db.AITarotReadingCounts.Add(readingCount);
        db.SaveChanges();
        return readingCount;
    }

    [Fact]
    public async Task Create_NewCardCount_CreatesReadingCount()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var sut = CreateSut(db);

        // Act
        var id = await sut.Handle(
            new CreateAiTarotReadingCountCommand(
                3,
                4,
                ["Past", "Present", "Future"],
                ["Quá khứ", "Hiện tại", "Tương lai"],
                300,
                400,
                false
            ),
            CancellationToken.None
        );

        // Assert
        var readingCount = await db.AITarotReadingCounts.SingleAsync();
        readingCount.Id.Should().Be(id);
        readingCount.CardCount.Should().Be(3);
        readingCount.WhiteCoins.Should().Be(4);
        readingCount.PositionsEn.Should().Equal("Past", "Present", "Future");
        readingCount.PositionsVi.Should().Equal("Quá khứ", "Hiện tại", "Tương lai");
        readingCount.MinWords.Should().Be(300);
        readingCount.MaxWords.Should().Be(400);
        readingCount.IsEnabled.Should().BeFalse();
        readingCount.DeletedAt.Should().BeNull();
    }

    [Fact]
    public async Task Create_ActiveCardCount_ThrowsConflict()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        SeedReadingCount(db, 3);
        var sut = CreateSut(db);

        // Act
        var act = () =>
            sut.Handle(
                new CreateAiTarotReadingCountCommand(
                    3,
                    5,
                    ["A", "B", "C"],
                    ["A", "B", "C"],
                    300,
                    400,
                    true
                ),
                CancellationToken.None
            );

        // Assert
        var ex = await act.Should().ThrowAsync<ConflictException>();
        ex.Which.ErrorCode.Should().Be(AiTarotErrorCode.CardCountExists);
        db.AITarotReadingCounts.Should().ContainSingle();
    }

    [Fact]
    public async Task Create_DeletedCardCount_RevivesSameReadingCount()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var existing = SeedReadingCount(db, 5, deleted: true);
        var sut = CreateSut(db);

        // Act
        var id = await sut.Handle(
            new CreateAiTarotReadingCountCommand(
                5,
                6,
                ["A", "B", "C", "D", "E"],
                ["A", "B", "C", "D", "E"],
                500,
                700,
                false
            ),
            CancellationToken.None
        );

        // Assert
        var readingCount = db.AITarotReadingCounts.IgnoreQueryFilters().Single();
        id.Should().Be(existing.Id);
        readingCount.Id.Should().Be(existing.Id);
        readingCount.CardCount.Should().Be(5);
        readingCount.WhiteCoins.Should().Be(6);
        readingCount.PositionsEn.Should().Equal("A", "B", "C", "D", "E");
        readingCount.PositionsVi.Should().Equal("A", "B", "C", "D", "E");
        readingCount.MinWords.Should().Be(500);
        readingCount.MaxWords.Should().Be(700);
        readingCount.IsEnabled.Should().BeFalse();
        readingCount.DeletedAt.Should().BeNull();
        readingCount.CreatedAt.Should().Be(existing.CreatedAt);
    }
}
