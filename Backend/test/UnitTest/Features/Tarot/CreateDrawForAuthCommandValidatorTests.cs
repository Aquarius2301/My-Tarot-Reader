using FluentAssertions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Features.Tarot;
using Xunit;

namespace MyTarotReader.UnitTest.Features.Tarot;

public class CreateDrawForAuthCommandValidatorTests
{
    private const string ValidCard = "maj-00";

    private readonly CreateDrawForAuthCommandValidator _sut = new();

    [Fact]
    public void Validate_ValidCard_IsValid()
    {
        // Arrange
        var command = new CreateDrawForAuthCommand(ValidCard, false, Guid.NewGuid());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("fake-card")]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_InvalidCard_IsInvalidWithInvalidCardCode(string cardCode)
    {
        // Arrange
        var command = new CreateDrawForAuthCommand(cardCode, false, Guid.NewGuid());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(CreateDrawForAuthCommand.CardCode))
            .Which.ErrorMessage.Should()
            .Be(TarotReadingErrorCode.InvalidCardCode);
    }
}
