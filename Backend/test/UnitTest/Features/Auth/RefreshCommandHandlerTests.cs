using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Features.Auth;
using MyTarotReader.Application.Services;
using MyTarotReader.Application.Settings;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.UnitTest.Common;
using Xunit;
using WalletEntity = MyTarotReader.Domain.Entities.Wallet;

namespace MyTarotReader.UnitTest.Features.Auth;

public class RefreshCommandHandlerTests
{
    private const string AccessToken = "access-token";
    private const string RefreshToken = "refresh-token";

    private static readonly JwtSetting DefaultJwt = new()
    {
        SecretKey = "test-secret-key",
        Issuer = "test-issuer",
        Audience = "test-audience",
    };

    private static (RefreshCommandHandler Sut, AppDbContext Db) CreateSut()
    {
        var db = TestDbContextFactory.Create();

        var tokens = new Mock<IJwtTokenGeneratorService>();
        tokens.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns(AccessToken);
        tokens.Setup(t => t.GenerateRefreshToken()).Returns(RefreshToken);

        var sut = new RefreshCommandHandler(db, tokens.Object, Options.Create(DefaultJwt));

        return (sut, db);
    }

    private static User SeedUser(AppDbContext db, string email = "jane@example.com", bool deleted = false)
    {
        var user = new User
        {
            Email = email,
            ProviderKey = email,
            FullName = "Jane",
            Picture = "http://pic",
            Role = UserRole.Registered,
            DeletedAt = deleted ? DateTimeOffset.UtcNow : null,
        };
        db.Users.Add(user);
        db.Wallets.Add(new WalletEntity { UserId = user.Id });
        db.SaveChanges();
        return user;
    }

    private static void SeedRefreshToken(
        AppDbContext db,
        Guid userId,
        string token,
        string fingerprint,
        DateTimeOffset expiresAt,
        bool deleted = false
    )
    {
        db.RefreshTokens.Add(
            new RefreshToken
            {
                UserId = userId,
                Token = token,
                DeviceFingerprint = fingerprint,
                ExpiresAt = expiresAt,
                DeletedAt = deleted ? DateTimeOffset.UtcNow : null,
            }
        );
        db.SaveChanges();
    }

    [Fact]
    public async Task Refresh_TokenNotFound_ThrowsInvalidRefreshToken()
    {
        // Arrange
        var (sut, _) = CreateSut();

        // Act
        var act = () => sut.Handle(new RefreshCommand("does-not-exist", "device-1"), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<UnauthorizedException>();
        ex.Which.ErrorCode.Should().Be(AuthErrorCode.InvalidRefreshToken);
    }

    [Fact]
    public async Task Refresh_DeletedToken_RevokesAllAndThrows()
    {
        // Arrange
        var (sut, db) = CreateSut();
        var user = SeedUser(db);
        SeedRefreshToken(
            db,
            user.Id,
            "deleted-token",
            "device-1",
            DateTimeOffset.UtcNow.AddDays(1),
            deleted: true
        );
        SeedRefreshToken(db, user.Id, "active-token", "device-1", DateTimeOffset.UtcNow.AddDays(1));

        // Act
        var act = () => sut.Handle(new RefreshCommand("deleted-token", "device-1"), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<UnauthorizedException>();
        ex.Which.ErrorCode.Should().Be(AuthErrorCode.InvalidRefreshToken);
        db.RefreshTokens.IgnoreQueryFilters().Should().HaveCount(2);
        db.RefreshTokens.IgnoreQueryFilters().Should().OnlyContain(rt => rt.DeletedAt.HasValue);
    }

    [Fact]
    public async Task Refresh_DeletedUser_RevokesAllAndThrows()
    {
        // Arrange
        var (sut, db) = CreateSut();
        var user = SeedUser(db, deleted: true);
        SeedRefreshToken(db, user.Id, "deleted-user-token", "device-1", DateTimeOffset.UtcNow.AddDays(1));

        // Act
        var act = () =>
            sut.Handle(new RefreshCommand("deleted-user-token", "device-1"), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<UnauthorizedException>();
        ex.Which.ErrorCode.Should().Be(AuthErrorCode.InvalidRefreshToken);
        db.RefreshTokens.IgnoreQueryFilters().Single().DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Refresh_DeviceFingerprintMismatch_RevokesAllAndThrows()
    {
        // Arrange
        var (sut, db) = CreateSut();
        var user = SeedUser(db);
        SeedRefreshToken(db, user.Id, "device-a-token", "device-a", DateTimeOffset.UtcNow.AddDays(1));

        // Act
        var act = () => sut.Handle(new RefreshCommand("device-a-token", "device-b"), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<UnauthorizedException>();
        ex.Which.ErrorCode.Should().Be(AuthErrorCode.InvalidRefreshToken);
        db.RefreshTokens.IgnoreQueryFilters().Should().OnlyContain(rt => rt.DeletedAt.HasValue);
    }

    [Fact]
    public async Task Refresh_ExpiredToken_SoftDeletesTokenAndThrows()
    {
        // Arrange
        var (sut, db) = CreateSut();
        var user = SeedUser(db);
        SeedRefreshToken(db, user.Id, "expired-token", "device-1", DateTimeOffset.UtcNow.AddMinutes(-1));

        // Act
        var act = () => sut.Handle(new RefreshCommand("expired-token", "device-1"), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<UnauthorizedException>();
        ex.Which.ErrorCode.Should().Be(AuthErrorCode.InvalidRefreshToken);
        db.RefreshTokens.IgnoreQueryFilters().Single().DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Refresh_ValidToken_ReturnsNewTokens()
    {
        // Arrange
        var (sut, db) = CreateSut();
        var user = SeedUser(db);
        SeedRefreshToken(db, user.Id, "valid-token", "device-1", DateTimeOffset.UtcNow.AddDays(1));

        // Act
        var result = await sut.Handle(new RefreshCommand("valid-token", "device-1"), CancellationToken.None);

        // Assert
        result.AccessToken.Should().Be(AccessToken);
        result.RefreshToken.Should().Be(RefreshToken);
        db.RefreshTokens.IgnoreQueryFilters()
            .Single(rt => rt.Token == "valid-token")
            .DeletedAt.Should()
            .NotBeNull();
        db.RefreshTokens.Should().Contain(rt => rt.Token == RefreshToken && rt.UserId == user.Id);
    }

    [Fact]
    public async Task Refresh_ValidToken_RotatesTokenWithSameFingerprint()
    {
        // Arrange
        var (sut, db) = CreateSut();
        var user = SeedUser(db);
        SeedRefreshToken(db, user.Id, "valid-token", "device-1", DateTimeOffset.UtcNow.AddDays(1));

        // Act
        await sut.Handle(new RefreshCommand("valid-token", "device-1"), CancellationToken.None);

        // Assert
        var newToken = db.RefreshTokens.Single(rt => rt.Token == RefreshToken);
        newToken.DeviceFingerprint.Should().Be("device-1");
        newToken
            .ExpiresAt.Should()
            .BeCloseTo(
                DateTimeOffset.UtcNow.AddDays(DefaultJwt.RefreshTokenDurationDays),
                TimeSpan.FromMinutes(5)
            );
        db.RefreshTokens.Count(rt => !rt.DeletedAt.HasValue).Should().Be(1);
    }

    [Fact]
    public async Task Refresh_AlreadyRotatedToken_ThrowsInvalidRefreshToken()
    {
        // Arrange
        var (sut, db) = CreateSut();
        var user = SeedUser(db);
        SeedRefreshToken(
            db,
            user.Id,
            "rotated-token",
            "device-1",
            DateTimeOffset.UtcNow.AddDays(1),
            deleted: true
        );

        // Act
        var act = () => sut.Handle(new RefreshCommand("rotated-token", "device-1"), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<UnauthorizedException>();
        ex.Which.ErrorCode.Should().Be(AuthErrorCode.InvalidRefreshToken);
    }

    [Fact]
    public async Task Refresh_DeviceMismatch_RevokesOnlyTargetUsersTokens()
    {
        // Arrange
        var (sut, db) = CreateSut();
        var userA = SeedUser(db, "a@example.com");
        var userB = SeedUser(db, "b@example.com");
        SeedRefreshToken(db, userA.Id, "a-token", "device-a", DateTimeOffset.UtcNow.AddDays(1));
        SeedRefreshToken(db, userB.Id, "b-token", "device-b", DateTimeOffset.UtcNow.AddDays(1));

        // Act
        var act = () => sut.Handle(new RefreshCommand("a-token", "device-c"), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<UnauthorizedException>();
        ex.Which.ErrorCode.Should().Be(AuthErrorCode.InvalidRefreshToken);
        db.RefreshTokens.IgnoreQueryFilters()
            .Single(rt => rt.Token == "a-token")
            .DeletedAt.Should()
            .NotBeNull();
        db.RefreshTokens.Single(rt => rt.Token == "b-token").DeletedAt.Should().BeNull();
    }
}
