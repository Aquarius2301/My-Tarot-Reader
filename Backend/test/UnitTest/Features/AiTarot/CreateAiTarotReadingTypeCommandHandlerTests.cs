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

public class CreateAiTarotReadingTypeCommandHandlerTests
{
    private static CreateAiTarotReadingTypeCommandHandler CreateSut(AppDbContext db) => new(db);

    private static AiTarotReadingType SeedReadingType(
        AppDbContext db,
        string code,
        bool deleted = false,
        string nameEn = "Love",
        string nameVi = "Tình yêu",
        bool isEnabled = true
    )
    {
        var readingType = new AiTarotReadingType
        {
            Code = code,
            NameEn = nameEn,
            NameVi = nameVi,
            IsEnabled = isEnabled,
            CreatedAt = DateTimeOffset.UtcNow,
            DeletedAt = deleted ? DateTimeOffset.UtcNow : null,
        };
        db.AITarotReadingTypes.Add(readingType);
        db.SaveChanges();
        return readingType;
    }

    [Fact]
    public async Task Create_NewCode_NormalizesAndCreatesReadingType()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var sut = CreateSut(db);

        // Act
        var id = await sut.Handle(
            new CreateAiTarotReadingTypeCommand("  LOVE  ", "Love", "Tình yêu", false),
            CancellationToken.None
        );

        // Assert
        var readingType = await db.AITarotReadingTypes.SingleAsync();
        readingType.Id.Should().Be(id);
        readingType.Code.Should().Be("love");
        readingType.NameEn.Should().Be("Love");
        readingType.NameVi.Should().Be("Tình yêu");
        readingType.IsEnabled.Should().BeFalse();
        readingType.DeletedAt.Should().BeNull();
    }

    [Fact]
    public async Task Create_ActiveCodeDifferentCase_ThrowsConflict()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        SeedReadingType(db, "love");
        var sut = CreateSut(db);

        // Act
        var act = () =>
            sut.Handle(
                new CreateAiTarotReadingTypeCommand("LOVE", "Love", "Tình yêu", true),
                CancellationToken.None
            );

        // Assert
        var ex = await act.Should().ThrowAsync<ConflictException>();
        ex.Which.ErrorCode.Should().Be(AiTarotErrorCode.ReadingTypeCodeExists);
        db.AITarotReadingTypes.Should().ContainSingle();
    }

    [Fact]
    public async Task Create_DeletedCodeDifferentCaseAndWhitespace_RevivesSameReadingType()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var existing = SeedReadingType(db, "Love", deleted: true);
        var sut = CreateSut(db);

        // Act
        var id = await sut.Handle(
            new CreateAiTarotReadingTypeCommand("  LOVE  ", "Love EN", "Tình yêu VI", false),
            CancellationToken.None
        );

        // Assert
        var readingType = db.AITarotReadingTypes.IgnoreQueryFilters().Single();
        id.Should().Be(existing.Id);
        readingType.Id.Should().Be(existing.Id);
        readingType.Code.Should().Be("love");
        readingType.NameEn.Should().Be("Love EN");
        readingType.NameVi.Should().Be("Tình yêu VI");
        readingType.IsEnabled.Should().BeFalse();
        readingType.DeletedAt.Should().BeNull();
        readingType.CreatedAt.Should().Be(existing.CreatedAt);
    }
}
