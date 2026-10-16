using FluentAssertions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Features.Auth;
using Xunit;

namespace MyTarotReader.UnitTest.Features.Auth;

public class GoogleLoginCommandValidatorTests
{
    private readonly GoogleLoginCommandValidator _sut = new();

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        // Arrange
        var command = new GoogleLoginCommand("credential", "vi", "device-1");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("en")]
    [InlineData("vi")]
    public void Validate_SupportedLocale_IsValid(string locale)
    {
        // Arrange
        var command = new GoogleLoginCommand("credential", locale, "device-1");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_NullLocale_IsValid()
    {
        // Arrange
        var command = new GoogleLoginCommand("credential", null, "device-1");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyCredential_IsInvalidWithInvalidKeyCredential()
    {
        // Arrange
        var command = new GoogleLoginCommand("", "vi", "device-1");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(GoogleLoginCommand.Credential))
            .Which.ErrorMessage.Should()
            .Be(AuthErrorCode.InvalidKeyCredential);
    }

    [Fact]
    public void Validate_EmptyDeviceFingerprint_IsInvalidWithInvalidDeviceFingerprint()
    {
        // Arrange
        var command = new GoogleLoginCommand("credential", "vi", "");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(GoogleLoginCommand.DeviceFingerprint))
            .Which.ErrorMessage.Should()
            .Be(AuthErrorCode.InvalidDeviceFingerprint);
    }

    [Fact]
    public void Validate_UnsupportedLocale_IsInvalidWithInvalidLocale()
    {
        // Arrange
        var command = new GoogleLoginCommand("credential", "fr", "device-1");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(GoogleLoginCommand.Locale))
            .Which.ErrorMessage.Should()
            .Be(AuthErrorCode.InvalidLocale);
    }
}
