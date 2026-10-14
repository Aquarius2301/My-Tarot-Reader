using FluentAssertions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Features.Tarot;
using Xunit;

namespace MyTarotReader.UnitTest.Features.Tarot;

public class CreateDrawForGuestCommandValidatorTests
{
    private const string ValidCard = "maj-00";

    private readonly CreateDrawForGuestCommandValidator _sut = new();

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        // Arrange
        var command = new CreateDrawForGuestCommand(ValidCard, false, "guest-1");

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
        var command = new CreateDrawForGuestCommand(cardCode, false, "guest-1");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(CreateDrawForGuestCommand.CardCode)
                && e.ErrorMessage == TarotReadingErrorCode.InvalidCardCode
            );
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyGuestKey_IsInvalidWithInvalidGuestKey(string guestKey)
    {
        // Arrange
        var command = new CreateDrawForGuestCommand(ValidCard, false, guestKey);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(CreateDrawForGuestCommand.GuestKey))
            .Which.ErrorMessage.Should()
            .Be(TarotReadingErrorCode.InvalidGuestKey);
    }
}
