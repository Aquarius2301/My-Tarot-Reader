using FluentAssertions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Features.Auth;
using Xunit;

namespace MyTarotReader.UnitTest.Features.Auth;

public class RefreshCommandValidatorTests
{
    private readonly RefreshCommandValidator _sut = new();

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        // Arrange
        var command = new RefreshCommand("refresh-token", "device-1");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyRefreshToken_IsInvalidWithInvalidRefreshToken()
    {
        // Arrange
        var command = new RefreshCommand("", "device-1");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(RefreshCommand.RefreshToken))
            .Which.ErrorMessage.Should()
            .Be(AuthErrorCode.InvalidRefreshToken);
    }

    [Fact]
    public void Validate_EmptyDeviceFingerprint_IsInvalidWithInvalidDeviceFingerprint()
    {
        // Arrange
        var command = new RefreshCommand("refresh-token", "");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(RefreshCommand.DeviceFingerprint))
            .Which.ErrorMessage.Should()
            .Be(AuthErrorCode.InvalidDeviceFingerprint);
    }
}
