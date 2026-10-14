using FluentAssertions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Features.AiTarot;
using MyTarotReader.Domain.Enums;
using Xunit;

namespace MyTarotReader.UnitTest.Features.AiTarot;

public class CreateAiTarotReadingCommandValidatorTests
{
    private readonly CreateAiTarotReadingCommandValidator _sut = new();

    private static CreateAiTarotReadingCommand CommandWithLocale(string? locale) =>
        new(
            CardCount.Three,
            QuestionType.Love,
            locale!,
            [new AiReadingCard("maj-00", false), new("maj-06", true), new("min-cups-2", false)],
            Guid.NewGuid()
        );

    [Theory]
    [InlineData("en")]
    [InlineData("vi")]
    public void Validate_SupportedLocale_IsValid(string locale)
    {
        // Arrange
        var command = CommandWithLocale(locale);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_NullLocale_IsValid()
    {
        // Arrange
        var command = CommandWithLocale(null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_UnsupportedLocale_IsInvalidWithInvalidLocale()
    {
        // Arrange
        var command = CommandWithLocale("fr");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(CreateAiTarotReadingCommand.Locale))
            .Which.ErrorMessage.Should()
            .Be(AiTarotErrorCode.InvalidLocale);
    }
}
