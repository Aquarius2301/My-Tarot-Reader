using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Contracts.Backgrounds;
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
/// Unit tests for <see cref="AuthService"/>, running against a real
/// <see cref="AppDbContext"/> with the EF Core InMemory provider so that
/// query filters, <c>IgnoreQueryFilters</c>, projections and aggregates behave
/// like production. Only the external seams (email queue, Google validator,
/// token generator) are mocked.
/// </summary>
public class AuthServiceTests
{
    private const string AccessToken = "access-token";
    private const string RefreshToken = "refresh-token";

    private static readonly JwtSetting DefaultJwt = new()
    {
        SecretKey = "test-secret-key",
        Issuer = "test-issuer",
        Audience = "test-audience",
    };
    private static readonly WalletSetting DefaultWallet = new()
    {
        InitialWhiteCoins = 5,
        ExpireDays = 30,
    };

    private static readonly GooglePayload DefaultPayload = new(
        "google-1",
        "jane@example.com",
        "Jane Doe",
        "http://google-pic"
    );

    #region Helpers

    private static User DefaultUser() =>
        new()
        {
            Email = "jane@example.com",
            ProviderKey = "google-1",
            FullName = "Jane",
            Picture = "http://pic",
            Role = UserRole.Registered,
        };

    private static GoogleLoginRequest NewLoginRequest() => new("credential-google-1", "vi");

    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableServiceProviderCaching(false)
            .Options;
        return new AppDbContext(options);
    }

    private static (
        AuthService Service,
        AppDbContext Db,
        Mock<IEmailBackgroundQueue> EmailQueue,
        Mock<IGoogleAuthValidator> Google
    ) CreateSut(AppDbContext? db = null, Action<Mock<IGoogleAuthValidator>>? google = null)
    {
        var context = db ?? CreateInMemoryContext();

        var emailQueue = new Mock<IEmailBackgroundQueue>();
        emailQueue
            .Setup(q =>
                q.EnqueueAsync(It.IsAny<WelcomeEmailMessage>(), It.IsAny<CancellationToken>())
            )
            .Returns(ValueTask.CompletedTask);

        var googleMock = new Mock<IGoogleAuthValidator>();
        googleMock
            .Setup(g => g.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DefaultPayload);
        google?.Invoke(googleMock);

        var tokens = new Mock<IJwtTokenGenerator>();
        tokens.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns(AccessToken);
        tokens.Setup(t => t.GenerateRefreshToken()).Returns(RefreshToken);

        var service = new AuthService(
            Options.Create(DefaultJwt),
            Options.Create(DefaultWallet),
            context,
            emailQueue.Object,
            googleMock.Object,
            tokens.Object
        );

        return (service, context, emailQueue, googleMock);
    }

    private static async Task<(User User, Wallet Wallet)> SeedUserAsync(
        AppDbContext db,
        User? user = null,
        IReadOnlyList<WhiteCoinBatch>? walletBatches = null
    )
    {
        var entity = user ?? DefaultUser();
        var batches =
            walletBatches
            ??
            [
                new WhiteCoinBatch
                {
                    Amount = 5,
                    RemainingAmount = 5,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(30),
                },
            ];
        var wallet = new Wallet
        {
            UserId = entity.Id,
            RedCoin = 0,
            UpdatedAt = DateTimeOffset.UtcNow,
            WhiteCoinBatches = [.. batches],
        };
        db.Users.Add(entity);
        db.Wallets.Add(wallet);
        await db.SaveChangesAsync();
        return (entity, wallet);
    }

    private static async Task SeedRefreshToken(
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
        await db.SaveChangesAsync();
    }

    #endregion

    #region GoogleLoginAsync

    /// <summary>
    /// Google credential is invalid → validator returns null → BadRequestException
    /// with <see cref="AuthErrorCode.InvalidKeyCredential"/>; no user is created.
    /// </summary>
    [Fact]
    public async Task GoogleLoginAsync_InvalidCredential_ThrowsBadRequest()
    {
        var (service, db, _, _) = CreateSut(google: g =>
            g.Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GooglePayload?)null)
        );

        var act = async () => await service.GoogleLoginAsync(NewLoginRequest(), "device-1");

        await act.Should()
            .ThrowAsync<BadRequestException>()
            .Where(e => e.ErrorCode == AuthErrorCode.InvalidKeyCredential);
        db.Users.Should().HaveCount(0);
    }

    /// <summary>
    /// First login of a new Google user → a User row with Role=Registered is created
    /// and profile fields (name, email, picture, provider key) are taken from the payload.
    /// </summary>
    [Fact]
    public async Task GoogleLoginAsync_NewUser_CreatesUserWithRegisteredRole()
    {
        var (service, db, _, _) = CreateSut();

        await service.GoogleLoginAsync(NewLoginRequest(), "device-1");

        var user = Assert.Single(db.Users);
        user.Role.Should().Be(UserRole.Registered);
        user.FullName.Should().Be(DefaultPayload.Name);
        user.Email.Should().Be(DefaultPayload.Email);
        user.Picture.Should().Be(DefaultPayload.Picture);
        user.ProviderKey.Should().Be(DefaultPayload.ProviderKey);
    }

    /// <summary>
    /// First login also creates a Wallet for the new user with exactly one
    /// WhiteCoinBatch whose Amount/RemainingAmount equal <see cref="WalletSetting.InitialWhiteCoins"/>
    /// and whose ExpiredAt is now + <see cref="WalletSetting.ExpireDays"/>. The sign-up
    /// bonus is recorded as an <see cref="Order"/> of type <see cref="OrderType.FirstLogin"/>
    /// linking to that batch, and a fresh zeroed <see cref="Streak"/> is seeded for the user.
    /// </summary>
    [Fact]
    public async Task GoogleLoginAsync_NewUser_CreatesWalletWithInitialWhiteCoinBatch()
    {
        var before = DateTimeOffset.UtcNow;
        var (service, db, _, _) = CreateSut();

        await service.GoogleLoginAsync(NewLoginRequest(), "device-1");
        var after = DateTimeOffset.UtcNow;

        var user = Assert.Single(db.Users);
        var wallet = Assert.Single(db.Wallets);
        wallet.UserId.Should().Be(user.Id);

        var batch = Assert.Single(wallet.WhiteCoinBatches);
        batch.Amount.Should().Be(DefaultWallet.InitialWhiteCoins);
        batch.RemainingAmount.Should().Be(DefaultWallet.InitialWhiteCoins);
        batch
            .ExpiredAt.Should()
            .BeOnOrAfter(before.AddDays(DefaultWallet.ExpireDays))
            .And.BeOnOrBefore(after.AddDays(DefaultWallet.ExpireDays));

        var order = db.Orders.Include(o => o.OrderDetails).Single();
        order.UserId.Should().Be(user.Id);
        order.Type.Should().Be(OrderType.FirstLogin);
        order.Amount.Should().Be(DefaultWallet.InitialWhiteCoins);
        order.Description.Should().NotBeNullOrWhiteSpace();
        var detail = Assert.Single(order.OrderDetails);
        detail.WhiteCoinBatchId.Should().Be(batch.Id);
        detail.Amount.Should().Be(DefaultWallet.InitialWhiteCoins);

        var streak = Assert.Single(db.Streaks);
        streak.UserId.Should().Be(user.Id);
        streak.CurrentStreak.Should().Be(0);
        streak.LongestStreak.Should().Be(0);
        streak.CycleDay.Should().Be(0);
        streak.IsSaverUsed.Should().BeFalse();
    }

    /// <summary>
    /// A brand-new user queues exactly one welcome email (delivered later in the
    /// background), addressed to the Google payload email, in the requested locale.
    /// </summary>
    [Fact]
    public async Task GoogleLoginAsync_NewUser_QueuesWelcomeEmailOnce()
    {
        var (service, _, emailQueue, _) = CreateSut();

        await service.GoogleLoginAsync(new GoogleLoginRequest("credential", "en"), "device-1");

        emailQueue.Verify(
            q =>
                q.EnqueueAsync(
                    new WelcomeEmailMessage(DefaultPayload.Email, DefaultPayload.Name, "en"),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    /// <summary>
    /// Login returns a <see cref="GoogleLoginResult"/> carrying the generated tokens and
    /// the configured access-token/refresh-token durations.
    /// </summary>
    [Fact]
    public async Task GoogleLoginAsync_NewUser_ReturnsTokensAndDurations()
    {
        var (service, _, _, _) = CreateSut();

        var result = await service.GoogleLoginAsync(NewLoginRequest(), "device-1");

        result.AccessToken.Should().Be(AccessToken);
        result.RefreshToken.Should().Be(RefreshToken);
        result.AccessTokenMinutes.Should().Be(DefaultJwt.AccessTokenDurationMinutes);
        result.RefreshTokenDays.Should().Be(DefaultJwt.RefreshTokenDurationDays);
    }

    /// <summary>
    /// Returning user (same ProviderKey) is not duplicated; only FullName and Picture
    /// are re-synced from the latest Google payload.
    /// </summary>
    [Fact]
    public async Task GoogleLoginAsync_ExistingUser_SyncsProfileFields()
    {
        var (service, db, _, _) = CreateSut();
        await SeedUserAsync(
            db,
            new User
            {
                Email = DefaultPayload.Email,
                ProviderKey = DefaultPayload.ProviderKey,
                FullName = "Old Name",
                Picture = "http://old-pic",
                Role = UserRole.Pro,
            }
        );

        await service.GoogleLoginAsync(NewLoginRequest(), "device-1");

        var user = Assert.Single(db.Users);
        user.FullName.Should().Be(DefaultPayload.Name);
        user.Picture.Should().Be(DefaultPayload.Picture);
        user.ProviderKey.Should().Be(DefaultPayload.ProviderKey);
    }

    /// <summary>
    /// Returning user keeps its existing single Wallet (no second one is created),
    /// no welcome email is queued, and no first-login <see cref="Order"/> nor
    /// <see cref="Streak"/> is seeded for it.
    /// </summary>
    [Fact]
    public async Task GoogleLoginAsync_ExistingUser_DoesNotRecreateWalletAndNoWelcomeEmail()
    {
        var (service, db, emailQueue, _) = CreateSut();
        await SeedUserAsync(
            db,
            new User
            {
                Email = DefaultPayload.Email,
                ProviderKey = DefaultPayload.ProviderKey,
                FullName = "Jane",
                Picture = "http://pic",
                Role = UserRole.Registered,
            }
        );
        db.Wallets.Should().HaveCount(1);

        await service.GoogleLoginAsync(NewLoginRequest(), "device-1");

        db.Wallets.Should().HaveCount(1);
        emailQueue.Verify(
            q => q.EnqueueAsync(It.IsAny<WelcomeEmailMessage>(), It.IsAny<CancellationToken>()),
            Times.Never
        );

        db.Orders.Should().BeEmpty();
        db.Streaks.Should().BeEmpty();
    }

    /// <summary>
    /// If a refresh token already exists for the same user+device, login soft-deletes
    /// that token (rotating it) and issues a fresh one, so only a single active token
    /// remains per device.
    /// </summary>
    [Fact]
    public async Task GoogleLoginAsync_ExistingRefreshToken_SameDevice_RotatesOldToken()
    {
        var (service, db, _, _) = CreateSut();
        var (user, _) = await SeedUserAsync(db);
        await SeedRefreshToken(
            db,
            user.Id,
            "old-token",
            "device-1",
            DateTimeOffset.UtcNow.AddDays(-1)
        );

        var result = await service.GoogleLoginAsync(NewLoginRequest(), "device-1");

        var old = db.RefreshTokens.IgnoreQueryFilters().Single(rt => rt.Token == "old-token");
        old.DeletedAt.Should().NotBeNull();
        db.RefreshTokens.Should().ContainSingle(
            rt => rt.DeviceFingerprint == "device-1" && !rt.DeletedAt.HasValue
        );
        result.RefreshToken.Should().Be(RefreshToken);
    }

    /// <summary>
    /// Logging in from a different device keeps the device-A refresh token valid and
    /// additionally issues a second token for the new device.
    /// </summary>
    [Fact]
    public async Task GoogleLoginAsync_NewDevice_AddsSecondRefreshToken()
    {
        var (service, db, _, _) = CreateSut();
        var (user, _) = await SeedUserAsync(db);
        db.RefreshTokens.Add(
            new RefreshToken
            {
                UserId = user.Id,
                Token = "device-a-token",
                DeviceFingerprint = "device-a",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            }
        );
        await db.SaveChangesAsync();

        await service.GoogleLoginAsync(NewLoginRequest(), "device-b");

        db.RefreshTokens.Should().HaveCount(2);
        db.RefreshTokens.Should()
            .Contain(rt => rt.DeviceFingerprint == "device-a" && !rt.DeletedAt.HasValue);
        db.RefreshTokens.Should()
            .Contain(rt => rt.DeviceFingerprint == "device-b" && rt.Token == RefreshToken);
    }

    #endregion

    #region RefreshAsync

    /// <summary>
    /// Refreshing with a token that is not stored at all → UnauthorizedException
    /// with <see cref="AuthErrorCode.InvalidRefreshToken"/>.
    /// </summary>
    [Fact]
    public async Task RefreshAsync_TokenNotFound_ThrowsInvalidRefreshToken()
    {
        var (service, _, _, _) = CreateSut();

        var act = async () => await service.RefreshAsync("does-not-exist", "device-1");

        await act.Should()
            .ThrowAsync<UnauthorizedException>()
            .Where(e => e.ErrorCode == AuthErrorCode.InvalidRefreshToken);
    }

    /// <summary>
    /// Reusing an already-revoked (soft-deleted) refresh token is treated as token
    /// theft: all active tokens of that user are revoked and an UnauthorizedException
    /// is thrown.
    /// </summary>
    [Fact]
    public async Task RefreshAsync_DeletedToken_RevokesAllAndThrows()
    {
        var (service, db, _, _) = CreateSut();
        var (user, _) = await SeedUserAsync(db);
        await SeedRefreshToken(
            db,
            user.Id,
            "deleted-token",
            "device-1",
            DateTimeOffset.UtcNow.AddDays(1),
            deleted: true
        );
        await SeedRefreshToken(
            db,
            user.Id,
            "active-token",
            "device-1",
            DateTimeOffset.UtcNow.AddDays(1)
        );

        var act = async () => await service.RefreshAsync("deleted-token", "device-1");

        await act.Should()
            .ThrowAsync<UnauthorizedException>()
            .Where(e => e.ErrorCode == AuthErrorCode.InvalidRefreshToken);
        db.RefreshTokens.IgnoreQueryFilters().Should().HaveCount(2);
        db.RefreshTokens.IgnoreQueryFilters().Should().OnlyContain(rt => rt.DeletedAt.HasValue);
    }

    /// <summary>
    /// Refreshing a token whose owner account has been soft-deleted also revokes all
    /// that user's tokens and throws UnauthorizedException.
    /// </summary>
    [Fact]
    public async Task RefreshAsync_DeletedUser_RevokesAllAndThrows()
    {
        var (service, db, _, _) = CreateSut();
        var deleted = DefaultUser();
        deleted.DeletedAt = DateTimeOffset.UtcNow;
        await SeedUserAsync(db, deleted);
        await SeedRefreshToken(
            db,
            deleted.Id,
            "deleted-user-token",
            "device-1",
            DateTimeOffset.UtcNow.AddDays(1)
        );

        var act = async () => await service.RefreshAsync("deleted-user-token", "device-1");

        await act.Should()
            .ThrowAsync<UnauthorizedException>()
            .Where(e => e.ErrorCode == AuthErrorCode.InvalidRefreshToken);
        db.RefreshTokens.IgnoreQueryFilters().Single().DeletedAt.Should().NotBeNull();
    }

    /// <summary>
    /// A refresh token presented from a different device fingerprint is a possible
    /// theft → all the user's active tokens are revoked and an UnauthorizedException
    /// is thrown.
    /// </summary>
    [Fact]
    public async Task RefreshAsync_DeviceFingerprintMismatch_RevokesAllAndThrows()
    {
        var (service, db, _, _) = CreateSut();
        var (user, _) = await SeedUserAsync(db);
        await SeedRefreshToken(
            db,
            user.Id,
            "device-a-token",
            "device-a",
            DateTimeOffset.UtcNow.AddDays(1)
        );

        var act = async () => await service.RefreshAsync("device-a-token", "device-b");

        await act.Should()
            .ThrowAsync<UnauthorizedException>()
            .Where(e => e.ErrorCode == AuthErrorCode.InvalidRefreshToken);
        db.RefreshTokens.IgnoreQueryFilters().Should().OnlyContain(rt => rt.DeletedAt.HasValue);
    }

    /// <summary>
    /// An expired-but-not-yet-revoked refresh token is soft-deleted (turned off) and
    /// an UnauthorizedException is thrown, forcing the client to log in again.
    /// </summary>
    [Fact]
    public async Task RefreshAsync_ExpiredToken_SoftDeletesTokenAndThrows()
    {
        var (service, db, _, _) = CreateSut();
        var (user, _) = await SeedUserAsync(db);
        await SeedRefreshToken(
            db,
            user.Id,
            "expired-token",
            "device-1",
            DateTimeOffset.UtcNow.AddMinutes(-1)
        );

        var act = async () => await service.RefreshAsync("expired-token", "device-1");

        await act.Should()
            .ThrowAsync<UnauthorizedException>()
            .Where(e => e.ErrorCode == AuthErrorCode.InvalidRefreshToken);
        db.RefreshTokens.IgnoreQueryFilters().Single().DeletedAt.Should().NotBeNull();
    }

    /// <summary>
    /// A valid, unexpired token on the matching device returns freshly generated
    /// access + refresh tokens while the old refresh token is soft-deleted.
    /// </summary>
    [Fact]
    public async Task RefreshAsync_ValidToken_ReturnsNewTokens()
    {
        var (service, db, _, _) = CreateSut();
        var (user, _) = await SeedUserAsync(db);
        await SeedRefreshToken(
            db,
            user.Id,
            "valid-token",
            "device-1",
            DateTimeOffset.UtcNow.AddDays(1)
        );

        var result = await service.RefreshAsync("valid-token", "device-1");

        result.AccessToken.Should().Be(AccessToken);
        result.RefreshToken.Should().Be(RefreshToken);
        db.RefreshTokens.IgnoreQueryFilters()
            .Single(rt => rt.Token == "valid-token")
            .DeletedAt.Should()
            .NotBeNull();
        db.RefreshTokens.Should().Contain(rt => rt.Token == RefreshToken && rt.UserId == user.Id);
    }

    /// <summary>
    /// Rotation: the newly issued refresh token is bound to the same device fingerprint,
    /// expires in <see cref="JwtSetting.RefreshTokenDurationDays"/>, and exactly one
    /// active token remains after rotation.
    /// </summary>
    [Fact]
    public async Task RefreshAsync_ValidToken_RotatesTokenWithSameFingerprint()
    {
        var (service, db, _, _) = CreateSut();
        var (user, _) = await SeedUserAsync(db);
        await SeedRefreshToken(
            db,
            user.Id,
            "valid-token",
            "device-1",
            DateTimeOffset.UtcNow.AddDays(1)
        );

        await service.RefreshAsync("valid-token", "device-1");

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

    /// <summary>
    /// Re-presenting a token that was already rotated (soft-deleted after a previous
    /// refresh) → UnauthorizedException, same as any revoked token.
    /// </summary>
    [Fact]
    public async Task RefreshAsync_AlreadyRotatedToken_ThrowsInvalidRefreshToken()
    {
        var (service, db, _, _) = CreateSut();
        var (user, _) = await SeedUserAsync(db);
        await SeedRefreshToken(
            db,
            user.Id,
            "rotated-token",
            "device-1",
            DateTimeOffset.UtcNow.AddDays(1),
            deleted: true
        );

        var act = async () => await service.RefreshAsync("rotated-token", "device-1");

        await act.Should()
            .ThrowAsync<UnauthorizedException>()
            .Where(e => e.ErrorCode == AuthErrorCode.InvalidRefreshToken);
    }

    /// <summary>
    /// A device-mismatch breach only revokes the affected user's tokens; other users'
    /// tokens (e.g. user B) stay active.
    /// </summary>
    [Fact]
    public async Task RefreshAsync_DeviceMismatch_RevokesOnlyTargetUsersTokens()
    {
        var (service, db, _, _) = CreateSut();
        var (userA, _) = await SeedUserAsync(
            db,
            new User
            {
                Email = "a@example.com",
                ProviderKey = "a",
                FullName = "A",
                Picture = "",
            }
        );
        var (userB, _) = await SeedUserAsync(
            db,
            new User
            {
                Email = "b@example.com",
                ProviderKey = "b",
                FullName = "B",
                Picture = "",
            }
        );
        await SeedRefreshToken(
            db,
            userA.Id,
            "a-token",
            "device-a",
            DateTimeOffset.UtcNow.AddDays(1)
        );
        await SeedRefreshToken(
            db,
            userB.Id,
            "b-token",
            "device-b",
            DateTimeOffset.UtcNow.AddDays(1)
        );

        var act = async () => await service.RefreshAsync("a-token", "device-c");

        await act.Should()
            .ThrowAsync<UnauthorizedException>()
            .Where(e => e.ErrorCode == AuthErrorCode.InvalidRefreshToken);
        db.RefreshTokens.IgnoreQueryFilters()
            .Single(rt => rt.Token == "a-token")
            .DeletedAt.Should()
            .NotBeNull();
        db.RefreshTokens.Single(rt => rt.Token == "b-token").DeletedAt.Should().BeNull();
    }

    #endregion

    #region LogoutAsync

    /// <summary>
    /// Logout soft-deletes the supplied refresh token and persists the change.
    /// </summary>
    [Fact]
    public async Task LogoutAsync_ExistingToken_SoftDeletesAndSaves()
    {
        var (service, db, _, _) = CreateSut();
        var (user, _) = await SeedUserAsync(db);
        await SeedRefreshToken(
            db,
            user.Id,
            "logout-token",
            "device-1",
            DateTimeOffset.UtcNow.AddDays(1)
        );

        await service.LogoutAsync("logout-token");

        db.RefreshTokens.IgnoreQueryFilters().Single().DeletedAt.Should().NotBeNull();
    }

    /// <summary>
    /// Logout with an unknown token does nothing and does not throw.
    /// </summary>
    [Fact]
    public async Task LogoutAsync_UnknownToken_IsNoOp()
    {
        var (service, db, _, _) = CreateSut();
        var (user, _) = await SeedUserAsync(db);
        await SeedRefreshToken(db, user.Id, "token", "device-1", DateTimeOffset.UtcNow.AddDays(1));

        await service.LogoutAsync("unknown-token");

        db.RefreshTokens.Single().DeletedAt.Should().BeNull();
    }

    #endregion

    #region GetCurrentUserAsync

    /// <summary>
    /// Requesting the current user for an unknown id → UnauthorizedException (default
    /// <see cref="SystemErrorCode.Unauthorized"/>).
    /// </summary>
    [Fact]
    public async Task GetCurrentUserAsync_UserNotFound_ThrowsUnauthorized()
    {
        var (service, _, _, _) = CreateSut();

        var act = async () => await service.GetCurrentUserAsync(Guid.NewGuid());

        await act.Should()
            .ThrowAsync<UnauthorizedException>()
            .Where(e => e.ErrorCode == SystemErrorCode.Unauthorized);
    }

    /// <summary>
    /// End-to-end grant check: after a new user logs in, the signup coins flow through to
    /// <c>GetCurrentUserAsync</c>. Regresses the bug where the welcome batch was inserted
    /// with <c>RemainingAmount == 0</c> and therefore never counted.
    /// </summary>
    [Fact]
    public async Task GetCurrentUserAsync_AfterGoogleLogin_ReportsInitialWhiteCoins()
    {
        var (service, db, _, _) = CreateSut();

        await service.GoogleLoginAsync(NewLoginRequest(), "device-1");

        var userId = db.Users.Single().Id;
        var result = await service.GetCurrentUserAsync(userId);

        result.WhiteCoin.Should().Be(DefaultWallet.InitialWhiteCoins);
    }

    /// <summary>
    /// <c>WhiteCoin</c> is the sum of RemainingAmount across active (non-expired) batches
    /// with a positive remaining amount; expired batches or batch rows are excluded.
    /// </summary>
    [Fact]
    public async Task GetCurrentUserAsync_ReturnsWhiteCoinSumOfActiveBatches()
    {
        var (service, db, _, _) = CreateSut();
        var (user, _) = await SeedUserAsync(
            db,
            walletBatches:
            [
                new()
                {
                    Amount = 5,
                    RemainingAmount = 5,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(30),
                },
                new()
                {
                    Amount = 3,
                    RemainingAmount = 3,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(10),
                },
                new()
                {
                    Amount = 2,
                    RemainingAmount = 2,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(-1),
                },
                new()
                {
                    Amount = 4,
                    RemainingAmount = 0,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(10),
                },
            ]
        );

        var result = await service.GetCurrentUserAsync(user.Id);

        result.WhiteCoin.Should().Be(8);
    }

    /// <summary>
    /// The result carries the user's RedCoin balance from the wallet and the Role.
    /// </summary>
    [Fact]
    public async Task GetCurrentUserAsync_ReturnsRedCoinAndRole()
    {
        var (service, db, _, _) = CreateSut();
        var (user, wallet) = await SeedUserAsync(
            db,
            new User
            {
                Email = "jane@example.com",
                ProviderKey = "google-1",
                FullName = "Jane",
                Picture = "http://pic",
                Role = UserRole.Pro,
            }
        );
        wallet.RedCoin = 7;
        await db.SaveChangesAsync();

        var result = await service.GetCurrentUserAsync(user.Id);

        result.RedCoin.Should().Be(7);
        result.Role.Should().Be(UserRole.Pro);
    }

    /// <summary>
    /// Batches that are already expired or fully spent (RemainingAmount == 0) do not
    /// contribute to <c>WhiteCoin</c>.
    /// </summary>
    [Fact]
    public async Task GetCurrentUserAsync_ExpiredOrDepletedBatchesExcluded()
    {
        var (service, db, _, _) = CreateSut();
        var (user, _) = await SeedUserAsync(
            db,
            walletBatches:
            [
                new()
                {
                    Amount = 5,
                    RemainingAmount = 5,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(30),
                },
                new()
                {
                    Amount = 3,
                    RemainingAmount = 3,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(-5),
                },
                new()
                {
                    Amount = 2,
                    RemainingAmount = 0,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(20),
                },
            ]
        );

        var result = await service.GetCurrentUserAsync(user.Id);

        result.WhiteCoin.Should().Be(5);
    }

    #endregion
}
