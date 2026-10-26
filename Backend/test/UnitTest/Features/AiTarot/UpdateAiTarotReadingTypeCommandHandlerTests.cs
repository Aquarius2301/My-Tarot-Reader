using FluentAssertions;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Features.AiTarot;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.UnitTest.Common;
using Xunit;

namespace MyTarotReader.UnitTest.Features.AiTarot;

public class UpdateAiTarotReadingTypeCommandHandlerTests
{
    private static UpdateAiTarotReadingTypeCommandHandler CreateSut(AppDbContext db) => new(db);

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
    public async Task Update_ExistingType_NormalizesCodeAndUpdatesFields()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var readingType = SeedReadingType(db, "Love");
        var sut = CreateSut(db);

        // Act
        await sut.Handle(
            new UpdateAiTarotReadingTypeCommand(
                readingType.Id,
                "  LOVE  ",
                "Love EN",
                "Tình yêu VI",
                false
            ),
            CancellationToken.None
        );

        // Assert
        var updated = db.AITarotReadingTypes.Single();
        updated.Code.Should().Be("love");
        updated.NameEn.Should().Be("Love EN");
        updated.NameVi.Should().Be("Tình yêu VI");
        updated.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task Update_TypeNotFound_ThrowsNotFound()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var sut = CreateSut(db);

        // Act
        var act = () =>
            sut.Handle(
                new UpdateAiTarotReadingTypeCommand(
                    Guid.NewGuid(),
                    "love",
                    "Love",
                    "Tình yêu",
                    true
                ),
                CancellationToken.None
            );

        // Assert
        var ex = await act.Should().ThrowAsync<NotFoundException>();
        ex.Which.ErrorCode.Should().Be(AiTarotErrorCode.ReadingTypeNotFound);
    }

    [Fact]
    public async Task Update_ActiveCodeConflict_ThrowsConflict()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var readingType = SeedReadingType(db, "love");
        SeedReadingType(db, "career", nameEn: "Career", nameVi: "Sự nghiệp");
        var sut = CreateSut(db);

        // Act
        var act = () =>
            sut.Handle(
                new UpdateAiTarotReadingTypeCommand(
                    readingType.Id,
                    "CAREER",
                    "Love",
                    "Tình yêu",
                    true
                ),
                CancellationToken.None
            );

        // Assert
        var ex = await act.Should().ThrowAsync<ConflictException>();
        ex.Which.ErrorCode.Should().Be(AiTarotErrorCode.ReadingTypeCodeExists);
        db.AITarotReadingTypes.Single(x => x.Id == readingType.Id).Code.Should().Be("love");
    }

    [Fact]
    public async Task Update_DeletedCodeConflict_ThrowsConflict()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var readingType = SeedReadingType(db, "love");
        SeedReadingType(
            db,
            "career",
            deleted: true,
            nameEn: "Career",
            nameVi: "Sự nghiệp"
        );
        var sut = CreateSut(db);

        // Act
        var act = () =>
            sut.Handle(
                new UpdateAiTarotReadingTypeCommand(
                    readingType.Id,
                    "CAREER",
                    "Love",
                    "Tình yêu",
                    true
                ),
                CancellationToken.None
            );

        // Assert
        var ex = await act.Should().ThrowAsync<ConflictException>();
        ex.Which.ErrorCode.Should().Be(AiTarotErrorCode.ReadingTypeCodeExists);
        db.AITarotReadingTypes.Single(x => x.Id == readingType.Id).Code.Should().Be("love");
    }
}
