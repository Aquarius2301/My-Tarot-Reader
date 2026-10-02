using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using MyTarotReader.Application.Contracts.Common;
using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Application.Settings;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.Infrastructure.Services;
using Xunit;

namespace MyTarotReader.UnitTest;

/// <summary>
/// Unit tests for <see cref="DevAuthService"/>, running against a real
/// <see cref="AppDbContext"/> with the EF Core InMemory provider so the seeding
/// and refresh-token rotation queries behave like production. Only the token
/// generator is mocked.
/// </summary>
public class DevAuthServiceTests
{
    private const string AccessToken = "dev-access-token";
    private const string RefreshToken = "dev-refresh-token";

    private static readonly JwtSetting DefaultJwt = new()
    {
        SecretKey = "test-secret-key",
        Issuer = "test-issuer",
        Audience = "test-audience",
    };

    private static readonly DevAuthSetting DefaultDevAuth = new()
    {
        ProviderKey = "dev-swagger",
        Email = "dev@my-tarot-reader.local",
        FullName = "Swagger Dev User",
        InitialWhiteCoins = 1000,
        WhiteCoinExpireDays = 365,
        DefaultDeviceFingerprint = "swagger-dev",
    };

    #region Helpers

    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableServiceProviderCaching(false)
            .Options;
        return new AppDbContext(options);
    }

    private static (DevAuthService Service, AppDbContext Db, Mock<IJwtTokenGenerator> Tokens)
        CreateSut(AppDbContext? db = null)
    {
        var context = db ?? CreateInMemoryContext();

        var tokens = new Mock<IJwtTokenGenerator>();
        tokens.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns(AccessToken);
        tokens.Setup(t => t.GenerateRefreshToken()).Returns(RefreshToken);

        var service = new DevAuthService(
            Options.Create(DefaultJwt),
            Options.Create(DefaultDevAuth),
            context,
            tokens.Object
        );

        return (service, context, tokens);
    }

    private static async Task<User> SeedExistingDevUserAsync(
        AppDbContext db,
        int remainingWhiteCoins = 500,
        int redCoin = 7
    )
    {
        var user = new User
        {
            Email = DefaultDevAuth.Email,
            ProviderKey = DefaultDevAuth.ProviderKey,
            FullName = DefaultDevAuth.FullName,
            Picture = string.Empty,
            Role = UserRole.Registered,
        };

        var wallet = new Wallet { UserId = user.Id, RedCoin = redCoin };
        var batch = new WhiteCoinBatch
        {
            WalletId = wallet.Id,
            Amount = remainingWhiteCoins,
            RemainingAmount = remainingWhiteCoins,
            ExpiredAt = DateTimeOffset.UtcNow.AddDays(30),
        };
        wallet.WhiteCoinBatches.Add(batch);

        db.Users.Add(user);
        db.Wallets.Add(wallet);
        await db.SaveChangesAsync();
        return user;
    }

    #endregion

    #region CreateDevTokenAsync — seeding

    /// <summary>
    /// No development user exists yet → one is created with the configured
    /// ProviderKey/Email/FullName and the default <see cref="UserRole.Registered"/> role.
    /// </summary>
    [Fact]
    public async Task CreateDevTokenAsync_NoExistingUser_SeedsUserFromSettings()
    {
        // Arrange
        var (service, db, _) = CreateSut();

        // Act
        var result = await service.CreateDevTokenAsync(
            new CreateDevTokenRequest(null),
            "device-1"
        );

        // Assert
        var user = Assert.Single(db.Users);
        user.ProviderKey.Should().Be(DefaultDevAuth.ProviderKey);
        user.Email.Should().Be(DefaultDevAuth.Email);
        user.FullName.Should().Be(DefaultDevAuth.FullName);
        user.Role.Should().Be(UserRole.Registered);
        result.UserId.Should().Be(user.Id);
    }

    /// <summary>
    /// The seeded user gets the same supporting rows a real Google registration
    /// creates: a wallet holding one white coin batch of
    /// <see cref="DevAuthSetting.InitialWhiteCoins"/>, a first-login order linking
    /// to that batch, and a zeroed streak.
    /// </summary>
    [Fact]
    public async Task CreateDevTokenAsync_NoExistingUser_SeedsWalletOrderAndStreak()
    {
        // Arrange
        var (service, db, _) = CreateSut();

        // Act
        await service.CreateDevTokenAsync(new CreateDevTokenRequest(null), "device-1");

        // Assert
        var user = Assert.Single(db.Users);
        var wallet = Assert.Single(db.Wallets);
        wallet.UserId.Should().Be(user.Id);

        var batch = Assert.Single(wallet.WhiteCoinBatches);
        batch.Amount.Should().Be(DefaultDevAuth.InitialWhiteCoins);
        batch.RemainingAmount.Should().Be(DefaultDevAuth.InitialWhiteCoins);
        batch.ExpiredAt.Should().BeOnOrAfter(
            DateTimeOffset.UtcNow.AddDays(DefaultDevAuth.WhiteCoinExpireDays - 1)
        );

        var order = db.Orders.Include(o => o.OrderDetails).Single();
        order.UserId.Should().Be(user.Id);
        order.Type.Should().Be(OrderType.FirstLogin);
        order.Amount.Should().Be(DefaultDevAuth.InitialWhiteCoins);
        var detail = Assert.Single(order.OrderDetails);
        detail.WhiteCoinBatchId.Should().Be(batch.Id);
        detail.Amount.Should().Be(DefaultDevAuth.InitialWhiteCoins);

        var streak = Assert.Single(db.Streaks);
        streak.UserId.Should().Be(user.Id);
        streak.CurrentStreak.Should().Be(0);
        streak.CycleDay.Should().Be(0);
        streak.IsSaverUsed.Should().BeFalse();
    }

    /// <summary>
    /// The result reports the seeded white coin balance so the caller knows how
    /// much coin budget the development user starts with.
    /// </summary>
    [Fact]
    public async Task CreateDevTokenAsync_NoExistingUser_ReportsSeededWhiteCoin()
    {
        // Arrange
        var (service, _, _) = CreateSut();

        // Act
        var result = await service.CreateDevTokenAsync(
            new CreateDevTokenRequest(null),
            "device-1"
        );

        // Assert
        result.WhiteCoin.Should().Be(DefaultDevAuth.InitialWhiteCoins);
        result.RedCoin.Should().Be(0);
    }

    /// <summary>
    /// A second call reuses the already-seeded user and does not create a second
    /// wallet, order or streak.
    /// </summary>
    [Fact]
    public async Task CreateDevTokenAsync_ExistingUser_ReusesUserWithoutReseeding()
    {
        // Arrange
        var (service, db, _) = CreateSut();
        var existing = await SeedExistingDevUserAsync(db);

        // Act
        var result = await service.CreateDevTokenAsync(
            new CreateDevTokenRequest(null),
            "device-1"
        );

        // Assert
        db.Users.Should().ContainSingle();
        db.Wallets.Should().ContainSingle();
        db.Orders.Should().BeEmpty();
        db.Streaks.Should().BeEmpty();
        result.UserId.Should().Be(existing.Id);
    }

    /// <summary>
    /// The white coin balance is read back from the database, so an existing
    /// development user reports its real (possibly already spent) balance
    /// together with its red coin balance.
    /// </summary>
    [Fact]
    public async Task CreateDevTokenAsync_ExistingUser_ReportsActualBalances()
    {
        // Arrange
        var (service, db, _) = CreateSut();
        await SeedExistingDevUserAsync(db, remainingWhiteCoins: 500, redCoin: 7);

        // Act
        var result = await service.CreateDevTokenAsync(
            new CreateDevTokenRequest(null),
            "device-1"
        );

        // Assert
        result.WhiteCoin.Should().Be(500);
        result.RedCoin.Should().Be(7);
    }

    /// <summary>
    /// A requested role is applied both when seeding and when updating an
    /// existing development user, so role-dependent behaviour is testable.
    /// </summary>
    [Fact]
    public async Task CreateDevTokenAsync_RoleRequested_AssignsRole()
    {
        // Arrange
        var (service, db, _) = CreateSut();

        // Act
        var result = await service.CreateDevTokenAsync(
            new CreateDevTokenRequest(UserRole.Pro),
            "device-1"
        );

        // Assert
        Assert.Single(db.Users).Role.Should().Be(UserRole.Pro);
        result.Role.Should().Be(UserRole.Pro);
    }

    /// <summary>
    /// An existing development user is promoted when a role is requested later.
    /// </summary>
    [Fact]
    public async Task CreateDevTokenAsync_ExistingUser_UpdatesRole()
    {
        // Arrange
        var (service, db, _) = CreateSut();
        await SeedExistingDevUserAsync(db);

        // Act
        var result = await service.CreateDevTokenAsync(
            new CreateDevTokenRequest(UserRole.Pro),
            "device-1"
        );

        // Assert
        db.Users.Single().Role.Should().Be(UserRole.Pro);
        result.Role.Should().Be(UserRole.Pro);
    }

    /// <summary>
    /// A null role leaves an existing user's role untouched, matching a real login.
    /// </summary>
    [Fact]
    public async Task CreateDevTokenAsync_ExistingUser_NullRole_KeepsCurrentRole()
    {
        // Arrange
        var (service, db, _) = CreateSut();
        var existing = await SeedExistingDevUserAsync(db);
        existing.Role = UserRole.Pro;
        await db.SaveChangesAsync();

        // Act
        var result = await service.CreateDevTokenAsync(
            new CreateDevTokenRequest(null),
            "device-1"
        );

        // Assert
        result.Role.Should().Be(UserRole.Pro);
    }

    #endregion

    #region CreateDevTokenAsync — tokens

    /// <summary>
    /// The result carries the generated tokens and the configured
    /// access-token/refresh-token durations.
    /// </summary>
    [Fact]
    public async Task CreateDevTokenAsync_ReturnsTokensAndDurations()
    {
        // Arrange
        var (service, _, _) = CreateSut();

        // Act
        var result = await service.CreateDevTokenAsync(
            new CreateDevTokenRequest(null),
            "device-1"
        );

        // Assert
        result.AccessToken.Should().Be(AccessToken);
        result.RefreshToken.Should().Be(RefreshToken);
        result.AccessTokenMinutes.Should().Be(DefaultJwt.AccessTokenDurationMinutes);
        result.RefreshTokenDays.Should().Be(DefaultJwt.RefreshTokenDurationDays);
    }

    /// <summary>
    /// The access token is generated for the development user, so its "sub" claim
    /// resolves to that user on JWT-protected endpoints.
    /// </summary>
    [Fact]
    public async Task CreateDevTokenAsync_GeneratesAccessTokenForDevUser()
    {
        // Arrange
        var (service, _, tokens) = CreateSut();

        // Act
        await service.CreateDevTokenAsync(new CreateDevTokenRequest(null), "device-1");

        // Assert
        tokens.Verify(
            t => t.GenerateAccessToken(It.Is<User>(u => u.ProviderKey == DefaultDevAuth.ProviderKey)),
            Times.Once
        );
    }

    /// <summary>
    /// The refresh token row is persisted bound to the requesting device and
    /// expires after <see cref="JwtSetting.RefreshTokenDurationDays"/>.
    /// </summary>
    [Fact]
    public async Task CreateDevTokenAsync_PersistsRefreshTokenBoundToDevice()
    {
        // Arrange
        var (service, db, _) = CreateSut();

        // Act
        var result = await service.CreateDevTokenAsync(
            new CreateDevTokenRequest(null),
            "device-1"
        );

        // Assert
        var stored = Assert.Single(db.RefreshTokens);
        stored.Token.Should().Be(RefreshToken);
        stored.DeviceFingerprint.Should().Be("device-1");
        stored.UserId.Should().Be(result.UserId);
        stored
            .ExpiresAt.Should()
            .BeCloseTo(
                DateTimeOffset.UtcNow.AddDays(DefaultJwt.RefreshTokenDurationDays),
                TimeSpan.FromMinutes(5)
            );
    }

    /// <summary>
    /// Calling twice from the same device rotates the refresh token instead of
    /// accumulating active tokens, mirroring real login behaviour.
    /// </summary>
    [Fact]
    public async Task CreateDevTokenAsync_SameDeviceTwice_RotatesRefreshToken()
    {
        // Arrange
        var (service, db, _) = CreateSut();
        await service.CreateDevTokenAsync(new CreateDevTokenRequest(null), "device-1");
        db.RefreshTokens
            .IgnoreQueryFilters()
            .Single()
            .Token = "first-token";

        // Act
        await service.CreateDevTokenAsync(new CreateDevTokenRequest(null), "device-1");

        // Assert
        var old = db.RefreshTokens.IgnoreQueryFilters().Single(rt => rt.Token == "first-token");
        old.DeletedAt.Should().NotBeNull();
        db.RefreshTokens.Should().ContainSingle(rt => !rt.DeletedAt.HasValue);
    }

    /// <summary>
    /// A blank or missing "X-Device-Id" header falls back to the configured default
    /// fingerprint, so the refresh token is still bound to a stable device.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateDevTokenAsync_BlankDeviceId_UsesDefaultFingerprint(string deviceId)
    {
        // Arrange
        var (service, db, _) = CreateSut();

        // Act
        await service.CreateDevTokenAsync(new CreateDevTokenRequest(null), deviceId);

        // Assert
        Assert
            .Single(db.RefreshTokens)
            .DeviceFingerprint.Should()
            .Be(DefaultDevAuth.DefaultDeviceFingerprint);
    }

    #endregion
}
