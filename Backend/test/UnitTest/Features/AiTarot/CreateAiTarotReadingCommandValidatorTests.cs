using FluentAssertions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Features.AiTarot;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.UnitTest.Common;
using Xunit;

namespace MyTarotReader.UnitTest.Features.AiTarot;

public class CreateAiTarotReadingCommandValidatorTests
{
    private readonly AppDbContext _db;
    private readonly CreateAiTarotReadingCommandValidator _sut;

    public CreateAiTarotReadingCommandValidatorTests()
    {
        _db = TestDbContextFactory.Create();
        SeedSettings(_db);
        _sut = new CreateAiTarotReadingCommandValidator(_db);
    }

    private static void SeedSettings(AppDbContext db)
    {
        db.AITarotReadingCounts.AddRange(
            new AITarotReadingCount
            {
                CardCount = 3,
                WhiteCoins = 2,
                PositionsEn = ["Core energy", "Challenges/obstacles", "Outcome/advice"],
                PositionsVi = ["Năng lượng cốt lõi", "Thử thách/trở ngại", "Kết quả/lời khuyên"],
                MinWords = 300,
                MaxWords = 400,
                IsEnabled = true,
            },
            new AITarotReadingCount
            {
                CardCount = 5,
                WhiteCoins = 3,
                PositionsEn = ["Core energy", "Challenges/obstacles", "Your strength", "Future", "Outcome/advice"],
                PositionsVi = ["Năng lượng cốt lõi", "Thử thách/trở ngại", "Điểm mạnh của bạn", "Tương lai", "Kết quả/lời khuyên"],
                MinWords = 450,
                MaxWords = 600,
                IsEnabled = true,
            }
        );
        db.AITarotReadingTypes.Add(
            new AiTarotReadingType
            {
                Code = "love",
                NameEn = "Love",
                NameVi = "Tình yêu",
                IsEnabled = true,
            }
        );
        db.SaveChanges();
    }

    private static CreateAiTarotReadingCommand CommandWithLocale(string? locale) =>
        new(
            3,
            "love",
            locale!,
            [new AiReadingCard("maj-00", false), new("maj-06", true), new("min-cups-2", false)],
            Guid.NewGuid()
        );

    [Theory]
    [InlineData("en")]
    [InlineData("vi")]
    public async Task Validate_SupportedLocale_IsValid(string locale)
    {
        // Arrange
        var command = CommandWithLocale(locale);

        // Act
        var result = await _sut.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_NullLocale_IsValid()
    {
        // Arrange
        var command = CommandWithLocale(null);

        // Act
        var result = await _sut.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_UnsupportedLocale_IsInvalidWithInvalidLocale()
    {
        // Arrange
        var command = CommandWithLocale("fr");

        // Act
        var result = await _sut.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(CreateAiTarotReadingCommand.Locale))
            .Which.ErrorMessage.Should()
            .Be(AiTarotErrorCode.InvalidLocale);
    }

    [Fact]
    public async Task Validate_UnknownCardCount_IsInvalidWithInvalidCardCount()
    {
        // Arrange
        var command = CommandWithLocale("en") with { CardCount = 99 };

        // Act
        var result = await _sut.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .Contain(e => e.ErrorMessage == AiTarotErrorCode.InvalidCardCount);
    }

    [Fact]
    public async Task Validate_UnknownQuestionType_IsInvalidWithInvalidQuestionType()
    {
        // Arrange
        var command = CommandWithLocale("en") with { Type = "unknown" };

        // Act
        var result = await _sut.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .Contain(e => e.ErrorMessage == AiTarotErrorCode.InvalidQuestionType);
    }

    [Fact]
    public async Task Validate_CardCountMismatch_IsInvalidWithInvalidCardCount()
    {
        // Arrange
        var command = CommandWithLocale("en") with
        {
            Cards = [new AiReadingCard("maj-00", false)],
        };

        // Act
        var result = await _sut.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .Contain(e => e.ErrorMessage == AiTarotErrorCode.InvalidCardCount);
    }

    [Fact]
    public async Task Validate_EmptyCards_IsInvalidWithInvalidCard()
    {
        // Arrange
        var command = CommandWithLocale("en") with { CardCount = 0, Cards = [] };

        // Act
        var result = await _sut.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .Contain(e => e.ErrorMessage == AiTarotErrorCode.InvalidCard);
    }

    [Fact]
    public async Task Validate_InvalidCardCode_IsInvalidWithInvalidCard()
    {
        // Arrange
        var command = CommandWithLocale("en") with
        {
            Cards =
            [
                new AiReadingCard("not-a-card", false),
                new AiReadingCard("maj-06", true),
                new AiReadingCard("min-cups-2", false),
            ],
        };

        // Act
        var result = await _sut.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .Contain(e => e.ErrorMessage == AiTarotErrorCode.InvalidCard);
    }
}
