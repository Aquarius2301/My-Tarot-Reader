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
using StreakEntity = MyTarotReader.Domain.Entities.Streak;
using WalletEntity = MyTarotReader.Domain.Entities.Wallet;

namespace MyTarotReader.UnitTest.Features.Auth;

public class GoogleLoginCommandHandlerTests
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

    private static GoogleLoginCommand NewCommand(string locale = "vi") =>
        new("credential-google-1", locale, "device-1");

    private static User DefaultUser() =>
        new()
        {
            Email = DefaultPayload.Email,
            ProviderKey = DefaultPayload.ProviderKey,
            FullName = "Jane",
            Picture = "http://pic",
            Role = UserRole.Registered,
        };

    private static (
        GoogleLoginCommandHandler Sut,
        AppDbContext Db,
        Mock<IEmailQueueService> EmailQueue,
        Mock<IGoogleAuthService> Google
    ) CreateSut(AppDbContext? db = null, Action<Mock<IGoogleAuthService>>? google = null)
    {
        var context = db ?? TestDbContextFactory.Create();

        var emailQueue = new Mock<IEmailQueueService>();
        emailQueue
            .Setup(q =>
                q.EnqueueAsync(It.IsAny<WelcomeEmailMessage>(), It.IsAny<CancellationToken>())
            )
            .Returns(ValueTask.CompletedTask);

        var googleMock = new Mock<IGoogleAuthService>();
        googleMock
            .Setup(g => g.ValidateAsync(It.IsAny<string>()))
            .ReturnsAsync(DefaultPayload);
        google?.Invoke(googleMock);

        var tokens = new Mock<IJwtTokenGeneratorService>();
        tokens.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns(AccessToken);
        tokens.Setup(t => t.GenerateRefreshToken()).Returns(RefreshToken);

        var sut = new GoogleLoginCommandHandler(
            context,
            tokens.Object,
            googleMock.Object,
            emailQueue.Object,
            Options.Create(DefaultWallet),
            Options.Create(DefaultJwt)
        );

        return (sut, context, emailQueue, googleMock);
    }

    private static void SeedUser(AppDbContext db, User user, params WhiteCoinBatch[] batches)
    {
        db.Users.Add(user);

        var wallet = new WalletEntity { UserId = user.Id, RedCoin = 0 };
        wallet.WhiteCoinBatches.AddRange(batches);
        db.Wallets.Add(wallet);

        db.SaveChanges();
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
    public async Task GoogleLogin_InvalidCredential_ThrowsBadRequest()
    {
        // Arrange
        var (sut, db, _, _) = CreateSut(google: g =>
            g.Setup(v => v.ValidateAsync(It.IsAny<string>())).ReturnsAsync((GooglePayload?)null)
        );

        // Act
        var act = () => sut.Handle(NewCommand(), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<BadRequestException>();
        ex.Which.ErrorCode.Should().Be(AuthErrorCode.InvalidKeyCredential);
        db.Users.Should().BeEmpty();
    }

    [Fact]
    public async Task GoogleLogin_NewUser_CreatesUserWithRegisteredRole()
    {
        // Arrange
        var (sut, db, _, _) = CreateSut();

        // Act
        await sut.Handle(NewCommand(), CancellationToken.None);

        // Assert
        var user = db.Users.Should().ContainSingle().Subject;
        user.Role.Should().Be(UserRole.Registered);
        user.FullName.Should().Be(DefaultPayload.Name);
        user.Email.Should().Be(DefaultPayload.Email);
        user.Picture.Should().Be(DefaultPayload.Picture);
        user.ProviderKey.Should().Be(DefaultPayload.ProviderKey);
    }

    [Fact]
    public async Task GoogleLogin_NewUser_CreatesWalletWithInitialWhiteCoinBatch()
    {
        // Arrange
        var before = DateTimeOffset.UtcNow;
        var (sut, db, _, _) = CreateSut();

        // Act
        await sut.Handle(NewCommand(), CancellationToken.None);
        var after = DateTimeOffset.UtcNow;

        // Assert
        var user = db.Users.Should().ContainSingle().Subject;
        var wallet = db.Wallets.Should().ContainSingle().Subject;
        wallet.UserId.Should().Be(user.Id);

        var batch = wallet.WhiteCoinBatches.Should().ContainSingle().Subject;
        batch.Amount.Should().Be(DefaultWallet.InitialWhiteCoins);
        batch.RemainingAmount.Should().Be(DefaultWallet.InitialWhiteCoins);
        batch
            .ExpiredAt.Should()
            .BeOnOrAfter(before.AddDays(DefaultWallet.ExpireDays))
            .And.BeOnOrBefore(after.AddDays(DefaultWallet.ExpireDays));

        var order = db.Orders.Should().ContainSingle().Subject;
        order.UserId.Should().Be(user.Id);
        order.Type.Should().Be(OrderType.FirstLogin);
        order.WhiteCoins.Should().Be(DefaultWallet.InitialWhiteCoins);
        order.Description.Should().NotBeNullOrWhiteSpace();

        var streak = db.Streaks.Should().ContainSingle().Subject;
        streak.UserId.Should().Be(user.Id);
        streak.CurrentStreak.Should().Be(0);
        streak.LongestStreak.Should().Be(0);
        streak.CycleDay.Should().Be(0);
        streak.IsSaverUsed.Should().BeFalse();
    }

    [Fact]
    public async Task GoogleLogin_NewUser_QueuesWelcomeEmailOnce()
    {
        // Arrange
        var (sut, _, emailQueue, _) = CreateSut();

        // Act
        await sut.Handle(NewCommand(locale: "en"), CancellationToken.None);

        // Assert
        emailQueue.Verify(
            q =>
                q.EnqueueAsync(
                    new WelcomeEmailMessage(DefaultPayload.Email, DefaultPayload.Name, "en"),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task GoogleLogin_NewUser_ReturnsTokensAndDurations()
    {
        // Arrange
        var (sut, _, _, _) = CreateSut();

        // Act
        var result = await sut.Handle(NewCommand(), CancellationToken.None);

        // Assert
        result.AccessToken.Should().Be(AccessToken);
        result.RefreshToken.Should().Be(RefreshToken);
        result.AccessTokenMinutes.Should().Be(DefaultJwt.AccessTokenDurationMinutes);
        result.RefreshTokenDays.Should().Be(DefaultJwt.RefreshTokenDurationDays);
    }

    [Fact]
    public async Task GoogleLogin_ExistingUser_SyncsProfileFields()
    {
        // Arrange
        var (sut, db, _, _) = CreateSut();
        SeedUser(
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

        // Act
        await sut.Handle(NewCommand(), CancellationToken.None);

        // Assert
        var user = db.Users.Should().ContainSingle().Subject;
        user.FullName.Should().Be(DefaultPayload.Name);
        user.Picture.Should().Be(DefaultPayload.Picture);
        user.ProviderKey.Should().Be(DefaultPayload.ProviderKey);
    }

    [Fact]
    public async Task GoogleLogin_ExistingUser_DoesNotRecreateWalletAndNoWelcomeEmail()
    {
        // Arrange
        var (sut, db, emailQueue, _) = CreateSut();
        SeedUser(db, DefaultUser());
        db.Wallets.Should().HaveCount(1);

        // Act
        await sut.Handle(NewCommand(), CancellationToken.None);

        // Assert
        db.Wallets.Should().HaveCount(1);
        emailQueue.Verify(
            q => q.EnqueueAsync(It.IsAny<WelcomeEmailMessage>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        db.Orders.Should().BeEmpty();
        db.Streaks.Should().BeEmpty();
    }

    [Fact]
    public async Task GoogleLogin_ExistingRefreshToken_SameDevice_RotatesOldToken()
    {
        // Arrange
        var (sut, db, _, _) = CreateSut();
        var user = DefaultUser();
        SeedUser(db, user);
        SeedRefreshToken(db, user.Id, "old-token", "device-1", DateTimeOffset.UtcNow.AddDays(-1));

        // Act
        var result = await sut.Handle(NewCommand(), CancellationToken.None);

        // Assert
        var old = db.RefreshTokens.IgnoreQueryFilters().Single(rt => rt.Token == "old-token");
        old.DeletedAt.Should().NotBeNull();
        db.RefreshTokens
            .Should()
            .ContainSingle(rt => rt.DeviceFingerprint == "device-1" && !rt.DeletedAt.HasValue);
        result.RefreshToken.Should().Be(RefreshToken);
    }

    [Fact]
    public async Task GoogleLogin_NewDevice_AddsSecondRefreshToken()
    {
        // Arrange
        var (sut, db, _, _) = CreateSut();
        var user = DefaultUser();
        SeedUser(db, user);
        db.RefreshTokens.Add(
            new RefreshToken
            {
                UserId = user.Id,
                Token = "device-a-token",
                DeviceFingerprint = "device-a",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            }
        );
        db.SaveChanges();

        // Act
        await sut.Handle(new GoogleLoginCommand("credential", "vi", "device-b"), CancellationToken.None);

        // Assert
        db.RefreshTokens.Should().HaveCount(2);
        db.RefreshTokens.Should()
            .Contain(rt => rt.DeviceFingerprint == "device-a" && !rt.DeletedAt.HasValue);
        db.RefreshTokens.Should()
            .Contain(rt => rt.DeviceFingerprint == "device-b" && rt.Token == RefreshToken);
    }
}
