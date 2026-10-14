using FluentAssertions;
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

public class GetCurrentUserQueryHandlerTests
{
    private static readonly WalletSetting DefaultWallet = new()
    {
        InitialWhiteCoins = 5,
        ExpireDays = 30,
    };

    private static GetCurrentUserHandler CreateSut(AppDbContext db) => new(db);

    private static User SeedUser(
        AppDbContext db,
        UserRole role = UserRole.Registered,
        int redCoin = 0,
        params WhiteCoinBatch[] batches
    )
    {
        var user = new User
        {
            Email = "jane@example.com",
            ProviderKey = "google-1",
            FullName = "Jane",
            Picture = "http://pic",
            Role = role,
        };
        var wallet = new WalletEntity { UserId = user.Id, RedCoin = redCoin };
        wallet.WhiteCoinBatches.AddRange(batches);

        db.Users.Add(user);
        db.Wallets.Add(wallet);
        db.SaveChanges();
        return user;
    }

    [Fact]
    public async Task GetCurrentUser_UserNotFound_ThrowsUnauthorized()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var sut = CreateSut(db);

        // Act
        var act = () => sut.Handle(new GetCurrentUserQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<UnauthorizedException>();
        ex.Which.ErrorCode.Should().Be(SystemErrorCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsWhiteCoinSumOfActiveBatches()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var user = SeedUser(
            db,
            batches:
            [
                new WhiteCoinBatch
                {
                    Amount = 5,
                    RemainingAmount = 5,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(30),
                },
                new WhiteCoinBatch
                {
                    Amount = 3,
                    RemainingAmount = 3,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(10),
                },
                new WhiteCoinBatch
                {
                    Amount = 2,
                    RemainingAmount = 2,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(-1),
                },
                new WhiteCoinBatch
                {
                    Amount = 4,
                    RemainingAmount = 0,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(10),
                },
            ]
        );
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetCurrentUserQuery(user.Id), CancellationToken.None);

        // Assert
        result.WhiteCoin.Should().Be(8);
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsRedCoinAndRole()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var user = SeedUser(
            db,
            role: UserRole.Pro,
            redCoin: 7,
            new WhiteCoinBatch
            {
                Amount = 5,
                RemainingAmount = 5,
                ExpiredAt = DateTimeOffset.UtcNow.AddDays(30),
            }
        );
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetCurrentUserQuery(user.Id), CancellationToken.None);

        // Assert
        result.RedCoin.Should().Be(7);
        result.Role.Should().Be(UserRole.Pro);
    }

    [Fact]
    public async Task GetCurrentUser_ExpiredOrDepletedBatchesExcluded()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var user = SeedUser(
            db,
            batches:
            [
                new WhiteCoinBatch
                {
                    Amount = 5,
                    RemainingAmount = 5,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(30),
                },
                new WhiteCoinBatch
                {
                    Amount = 3,
                    RemainingAmount = 3,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(-5),
                },
                new WhiteCoinBatch
                {
                    Amount = 2,
                    RemainingAmount = 0,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(20),
                },
            ]
        );
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetCurrentUserQuery(user.Id), CancellationToken.None);

        // Assert
        result.WhiteCoin.Should().Be(5);
    }

    [Fact]
    public async Task GetCurrentUser_AfterGoogleLogin_ReportsInitialWhiteCoins()
    {
        // Arrange
        var db = TestDbContextFactory.Create();

        var google = new Mock<IGoogleAuthService>();
        google
            .Setup(g => g.ValidateAsync(It.IsAny<string>()))
            .ReturnsAsync(new GooglePayload("google-1", "jane@example.com", "Jane Doe", "http://pic"));

        var emailQueue = new Mock<IEmailQueueService>();
        emailQueue
            .Setup(q =>
                q.EnqueueAsync(It.IsAny<WelcomeEmailMessage>(), It.IsAny<CancellationToken>())
            )
            .Returns(ValueTask.CompletedTask);

        var tokens = new Mock<IJwtTokenGeneratorService>();
        tokens.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("access");
        tokens.Setup(t => t.GenerateRefreshToken()).Returns("refresh");

        var login = new GoogleLoginCommandHandler(
            db,
            tokens.Object,
            google.Object,
            emailQueue.Object,
            Options.Create(DefaultWallet),
            Options.Create(new JwtSetting())
        );
        await login.Handle(new GoogleLoginCommand("credential", "vi", "device-1"), CancellationToken.None);

        var userId = db.Users.Single().Id;
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetCurrentUserQuery(userId), CancellationToken.None);

        // Assert
        result.WhiteCoin.Should().Be(DefaultWallet.InitialWhiteCoins);
    }
}
