using FluentAssertions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Features.Wallet;
using Xunit;

namespace MyTarotReader.UnitTest.Features.Wallet;

public class ConvertRedToWhiteCommandValidatorTests
{
    private readonly ConvertRedToWhiteCommandValidator _sut = new();

    [Fact]
    public void Validate_PositiveAmount_IsValid()
    {
        // Arrange
        var command = new ConvertRedToWhiteCommand(Guid.NewGuid(), 3);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_NonPositiveAmount_IsInvalidWithInvalidAmount(int redCoins)
    {
        // Arrange
        var command = new ConvertRedToWhiteCommand(Guid.NewGuid(), redCoins);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(ConvertRedToWhiteCommand.RedCoins))
            .Which.ErrorMessage.Should()
            .Be(WalletErrorCode.InvalidAmount);
    }
}
