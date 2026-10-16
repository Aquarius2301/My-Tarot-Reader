using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Features.Auth;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.UnitTest.Common;
using Xunit;
using WalletEntity = MyTarotReader.Domain.Entities.Wallet;

namespace MyTarotReader.UnitTest.Features.Auth;

public class LogoutCommandHandlerTests
{
    private static (LogoutCommandHandler Sut, AppDbContext Db) CreateSut()
    {
        var db = TestDbContextFactory.Create();
        return (new LogoutCommandHandler(db), db);
    }

    private static User SeedUser(AppDbContext db)
    {
        var user = new User
        {
            Email = "jane@example.com",
            ProviderKey = "google-1",
            FullName = "Jane",
            Picture = "http://pic",
            Role = UserRole.Registered,
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
        DateTimeOffset expiresAt
    )
    {
        db.RefreshTokens.Add(
            new RefreshToken
            {
                UserId = userId,
                Token = token,
                DeviceFingerprint = fingerprint,
                ExpiresAt = expiresAt,
            }
        );
        db.SaveChanges();
    }

    [Fact]
    public async Task Logout_ExistingToken_SoftDeletesAndSaves()
    {
        // Arrange
        var (sut, db) = CreateSut();
        var user = SeedUser(db);
        SeedRefreshToken(db, user.Id, "logout-token", "device-1", DateTimeOffset.UtcNow.AddDays(1));

        // Act
        await sut.Handle(new LogoutCommand("logout-token"), CancellationToken.None);

        // Assert
        db.RefreshTokens.IgnoreQueryFilters().Single().DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Logout_UnknownToken_IsNoOp()
    {
        // Arrange
        var (sut, db) = CreateSut();
        var user = SeedUser(db);
        SeedRefreshToken(db, user.Id, "token", "device-1", DateTimeOffset.UtcNow.AddDays(1));

        // Act
        await sut.Handle(new LogoutCommand("unknown-token"), CancellationToken.None);

        // Assert
        db.RefreshTokens.Single().DeletedAt.Should().BeNull();
    }
}
