using FluentAssertions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Features.DevAuth;
using MyTarotReader.Domain.Enums;
using Xunit;

namespace MyTarotReader.UnitTest.Features.DevAuth;

public class CreateDevTokenCommandValidatorTests
{
    private readonly CreateDevTokenCommandValidator _sut = new();

    [Fact]
    public void Validate_NoRole_IsValid()
    {
        // Arrange
        var cmd = new CreateDevTokenCommand(null, "device");

        // Act
        var result = _sut.Validate(cmd);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(UserRole.Registered)]
    [InlineData(UserRole.Pro)]
    public void Validate_ValidRole_IsValid(UserRole role)
    {
        // Arrange
        var cmd = new CreateDevTokenCommand(role, "device");

        // Act
        var result = _sut.Validate(cmd);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData((UserRole)99)]
    [InlineData((UserRole)123)]
    public void Validate_InvalidRole_IsInvalid(UserRole role)
    {
        // Arrange
        var cmd = new CreateDevTokenCommand(role, "device");

        // Act
        var result = _sut.Validate(cmd);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .ContainSingle()
            .Which.ErrorMessage.Should()
            .Be(DevAuthErrorCode.InvalidRole);
    }
}
