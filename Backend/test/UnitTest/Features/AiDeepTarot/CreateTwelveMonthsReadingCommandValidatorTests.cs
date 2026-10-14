using FluentAssertions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Features.AiDeepTarot;
using Xunit;

namespace MyTarotReader.UnitTest.Features.AiDeepTarot;

public class CreateTwelveMonthsReadingCommandValidatorTests
{
    private static readonly string[] TwelveCardCodes =
    [
        "maj-00",
        "maj-01",
        "maj-02",
        "maj-03",
        "maj-04",
        "maj-05",
        "maj-06",
        "maj-07",
        "maj-08",
        "maj-09",
        "maj-10",
        "maj-11",
    ];

    private readonly CreateTwelveMonthsReadingCommandValidator _sut = new();

    private static List<AiDeepCardRequest> Cards(int count) =>
        TwelveCardCodes.Take(count).Select(code => new AiDeepCardRequest(code, false)).ToList();

    private static CreateTwelveMonthsReadingCommand ValidCommand(string? locale = "vi") =>
        new(locale!, Cards(12), Guid.NewGuid());

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        // Act
        var result = _sut.Validate(ValidCommand());

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("en")]
    [InlineData("vi")]
    public void Validate_SupportedLocale_IsValid(string locale)
    {
        // Act
        var result = _sut.Validate(ValidCommand(locale));

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_NullLocale_IsValid()
    {
        // Act
        var result = _sut.Validate(ValidCommand(locale: null));

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_UnsupportedLocale_IsInvalidWithInvalidLocale()
    {
        // Act
        var result = _sut.Validate(ValidCommand(locale: "fr"));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(CreateTwelveMonthsReadingCommand.Locale))
            .Which.ErrorMessage.Should()
            .Be(AiDeepTarotErrorCode.InvalidLocale);
    }

    [Fact]
    public void Validate_CardCountMismatch_IsInvalidWithInvalidCardCount()
    {
        // Act
        var result = _sut.Validate(ValidCommand() with { Cards = Cards(10) });

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(CreateTwelveMonthsReadingCommand.Cards)
                && e.ErrorMessage == AiDeepTarotErrorCode.InvalidCardCount
            );
    }

    [Fact]
    public void Validate_NullCards_IsInvalidWithInvalidCardCount()
    {
        // Act
        var result = _sut.Validate(ValidCommand() with { Cards = null! });

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(CreateTwelveMonthsReadingCommand.Cards)
                && e.ErrorMessage == AiDeepTarotErrorCode.InvalidCardCount
            );
    }

    [Fact]
    public void Validate_InvalidCardCode_IsInvalidWithInvalidCard()
    {
        // Arrange
        var cards = Cards(12);
        cards[5] = new AiDeepCardRequest("fake-card", false);

        // Act
        var result = _sut.Validate(ValidCommand() with { Cards = cards });

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(CreateTwelveMonthsReadingCommand.Cards)
                && e.ErrorMessage == AiDeepTarotErrorCode.InvalidCard
            );
    }
}
