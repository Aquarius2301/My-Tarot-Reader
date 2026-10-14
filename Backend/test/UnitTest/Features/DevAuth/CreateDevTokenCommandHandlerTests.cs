using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using MyTarotReader.Application.Features.DevAuth;
using MyTarotReader.Application.Services;
using MyTarotReader.Application.Settings;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.UnitTest.Common;
using Xunit;
using WalletEntity = MyTarotReader.Domain.Entities.Wallet;

namespace MyTarotReader.UnitTest.Features.DevAuth;

public class CreateDevTokenCommandHandlerTests
{
    private static readonly JwtSetting Jwt = new()
    {
        SecretKey = "test-secret",
        Issuer = "test-issuer",
        Audience = "test-audience",
        AccessTokenDurationMinutes = 15,
        RefreshTokenDurationDays = 14,
    };

    private static readonly DevAuthSetting DevAuth = new()
    {
        ProviderKey = "dev-swagger",
        Email = "dev@my-tarot-reader.local",
        FullName = "Swagger Dev User",
        InitialWhiteCoins = 1000,
        WhiteCoinExpireDays = 365,
        DefaultDeviceFingerprint = "swagger-dev",
    };

    private static (CreateDevTokenCommandHandler Sut, AppDbContext Db, Mock<IJwtTokenGeneratorService> Jwt) CreateSut()
    {
        var db = TestDbContextFactory.Create();
        var jwt = new Mock<IJwtTokenGeneratorService>();
        jwt.Setup(x => x.GenerateAccessToken(It.IsAny<User>())).Returns("dev-access-token");
        jwt.Setup(x => x.GenerateRefreshToken()).Returns("dev-refresh-token");

        var sut = new CreateDevTokenCommandHandler(
            db,
            jwt.Object,
            Options.Create(Jwt),
            Options.Create(DevAuth)
        );

        return (sut, db, jwt);
    }

    private static User SeedExistingDevUser(
        AppDbContext db,
        int remainingWhiteCoins = 500,
        int redCoin = 7,
        UserRole role = UserRole.Registered
    )
    {
        var user = new User
        {
            FullName = DevAuth.FullName,
            Email = DevAuth.Email,
            Picture = string.Empty,
            ProviderKey = DevAuth.ProviderKey,
            Role = role,
        };
        db.Users.Add(user);

        var wallet = new WalletEntity { UserId = user.Id, RedCoin = redCoin };
        wallet.WhiteCoinBatches.Add(
            new WhiteCoinBatch
            {
                WalletId = wallet.Id,
                Amount = remainingWhiteCoins,
                RemainingAmount = remainingWhiteCoins,
                ExpiredAt = DateTimeOffset.UtcNow.AddDays(30),
            }
        );
        db.Wallets.Add(wallet);
        db.SaveChanges();
        return user;
    }

    [Fact]
    public async Task CreateDevToken_NoExistingUser_SeedsUserFromSettings()
    {
        // Arrange
        var (sut, db, _) = CreateSut();

        // Act
        var result = await sut.Handle(
            new CreateDevTokenCommand(null, "device-1"),
            CancellationToken.None
        );

        // Assert
        var user = db.Users.Should().ContainSingle().Subject;
        user.ProviderKey.Should().Be(DevAuth.ProviderKey);
        user.Email.Should().Be(DevAuth.Email);
        user.FullName.Should().Be(DevAuth.FullName);
        user.Role.Should().Be(UserRole.Registered);
        result.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task CreateDevToken_NoExistingUser_SeedsWalletOrderAndStreak()
    {
        // Arrange
        var (sut, db, _) = CreateSut();

        // Act
        await sut.Handle(new CreateDevTokenCommand(null, "device-1"), CancellationToken.None);

        // Assert
        var wallet = db.Wallets.Should().ContainSingle().Subject;
        var batch = wallet.WhiteCoinBatches.Should().ContainSingle().Subject;
        batch.Amount.Should().Be(DevAuth.InitialWhiteCoins);
        batch.RemainingAmount.Should().Be(DevAuth.InitialWhiteCoins);
        batch.ExpiredAt.Should().BeOnOrAfter(DateTimeOffset.UtcNow.AddDays(364));

        var order = db.Orders.Should().ContainSingle().Subject;
        order.Type.Should().Be(OrderType.FirstLogin);
        order.WhiteCoins.Should().Be(DevAuth.InitialWhiteCoins);

        var streak = db.Streaks.Should().ContainSingle().Subject;
        streak.CurrentStreak.Should().Be(0);
        streak.CycleDay.Should().Be(0);
    }

    [Fact]
    public async Task CreateDevToken_NoExistingUser_ReportsSeededWhiteCoin()
    {
        // Arrange
        var (sut, _, _) = CreateSut();

        // Act
        var result = await sut.Handle(
            new CreateDevTokenCommand(null, "device-1"),
            CancellationToken.None
        );

        // Assert
        result.WhiteCoin.Should().Be(DevAuth.InitialWhiteCoins);
        result.RedCoin.Should().Be(0);
    }

    [Fact]
    public async Task CreateDevToken_ExistingUser_ReusesUserWithoutReseeding()
    {
        // Arrange
        var (sut, db, _) = CreateSut();
        var existing = SeedExistingDevUser(db);

        // Act
        var result = await sut.Handle(
            new CreateDevTokenCommand(null, "device-1"),
            CancellationToken.None
        );

        // Assert
        db.Users.Should().ContainSingle();
        db.Wallets.Should().ContainSingle();
        db.Orders.Should().BeEmpty();
        db.Streaks.Should().BeEmpty();
        result.UserId.Should().Be(existing.Id);
    }

    [Fact]
    public async Task CreateDevToken_ExistingUser_ReportsActualBalances()
    {
        // Arrange
        var (sut, db, _) = CreateSut();
        SeedExistingDevUser(db, remainingWhiteCoins: 500, redCoin: 7);

        // Act
        var result = await sut.Handle(
            new CreateDevTokenCommand(null, "device-1"),
            CancellationToken.None
        );

        // Assert
        result.WhiteCoin.Should().Be(500);
        result.RedCoin.Should().Be(7);
    }

    [Fact]
    public async Task CreateDevToken_RoleRequested_AssignsRoleOnSeed()
    {
        // Arrange
        var (sut, db, _) = CreateSut();

        // Act
        var result = await sut.Handle(
            new CreateDevTokenCommand(UserRole.Pro, "device-1"),
            CancellationToken.None
        );

        // Assert
        db.Users.Should().ContainSingle().Which.Role.Should().Be(UserRole.Pro);
        result.Role.Should().Be(UserRole.Pro);
    }

    [Fact]
    public async Task CreateDevToken_ExistingUser_UpdatesRole()
    {
        // Arrange
        var (sut, db, _) = CreateSut();
        var existing = SeedExistingDevUser(db, role: UserRole.Registered);

        // Act
        var result = await sut.Handle(
            new CreateDevTokenCommand(UserRole.Pro, "device-1"),
            CancellationToken.None
        );

        // Assert
        existing.Role.Should().Be(UserRole.Pro);
        result.Role.Should().Be(UserRole.Pro);
    }

    [Fact]
    public async Task CreateDevToken_ExistingUser_NullRole_KeepsCurrentRole()
    {
        // Arrange
        var (sut, db, _) = CreateSut();
        var existing = SeedExistingDevUser(db, role: UserRole.Pro);

        // Act
        var result = await sut.Handle(new CreateDevTokenCommand(null, "device-1"), CancellationToken.None);

        // Assert
        existing.Role.Should().Be(UserRole.Pro);
        result.Role.Should().Be(UserRole.Pro);
    }

    [Fact]
    public async Task CreateDevToken_ReturnsTokensAndDurations()
    {
        // Arrange
        var (sut, _, _) = CreateSut();

        // Act
        var result = await sut.Handle(
            new CreateDevTokenCommand(null, "device-1"),
            CancellationToken.None
        );

        // Assert
        result.AccessToken.Should().Be("dev-access-token");
        result.RefreshToken.Should().Be("dev-refresh-token");
        result.AccessTokenMinutes.Should().Be(Jwt.AccessTokenDurationMinutes);
        result.RefreshTokenDays.Should().Be(Jwt.RefreshTokenDurationDays);
    }

    [Fact]
    public async Task CreateDevToken_GeneratesAccessTokenForDevUser()
    {
        // Arrange
        var (sut, _, jwt) = CreateSut();

        // Act
        await sut.Handle(new CreateDevTokenCommand(null, "device-1"), CancellationToken.None);

        // Assert
        jwt.Verify(
            x => x.GenerateAccessToken(It.Is<User>(u => u.ProviderKey == DevAuth.ProviderKey)),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateDevToken_PersistsRefreshTokenBoundToDevice()
    {
        // Arrange
        var (sut, db, _) = CreateSut();

        // Act
        await sut.Handle(new CreateDevTokenCommand(null, "device-1"), CancellationToken.None);

        // Assert
        var token = db.RefreshTokens.Should().ContainSingle().Subject;
        token.Token.Should().Be("dev-refresh-token");
        token.DeviceFingerprint.Should().Be("device-1");
        token.ExpiresAt.Should().BeCloseTo(DateTimeOffset.UtcNow.AddDays(14), TimeSpan.FromMinutes(5));
    }

    [Fact]
    public async Task CreateDevToken_SameDeviceTwice_RotatesRefreshToken()
    {
        // Arrange
        var (sut, db, _) = CreateSut();

        // Act
        await sut.Handle(new CreateDevTokenCommand(null, "device-1"), CancellationToken.None);
        await sut.Handle(new CreateDevTokenCommand(null, "device-1"), CancellationToken.None);

        // Assert
        db.RefreshTokens.IgnoreQueryFilters().Count().Should().Be(2);
        db.RefreshTokens.Should().ContainSingle(rt => !rt.DeletedAt.HasValue);
        db.RefreshTokens.IgnoreQueryFilters().Should().ContainSingle(rt => rt.DeletedAt.HasValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateDevToken_BlankDeviceId_UsesDefaultFingerprint(string deviceId)
    {
        // Arrange
        var (sut, db, _) = CreateSut();

        // Act
        await sut.Handle(new CreateDevTokenCommand(null, deviceId), CancellationToken.None);

        // Assert
        db.RefreshTokens
            .Should()
            .ContainSingle()
            .Which.DeviceFingerprint.Should()
            .Be(DevAuth.DefaultDeviceFingerprint);
    }
}
