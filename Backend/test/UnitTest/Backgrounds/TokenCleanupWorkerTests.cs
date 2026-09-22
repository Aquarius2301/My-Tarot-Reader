using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using MyTarotReader.Application.Contracts.Persistence;
using MyTarotReader.Application.Settings;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Infrastructure.Backgrounds;
using MyTarotReader.Infrastructure.Persistence;
using System.Reflection;
using Xunit;

namespace MyTarotReader.UnitTest.Backgrounds;

/// <summary>
/// Unit tests for <see cref="TokenCleanupWorker"/>.
/// </summary>
public class TokenCleanupWorkerTests
{
    private static readonly MethodInfo CleanupOnceAsyncMethod = typeof(TokenCleanupWorker)
        .GetMethod("CleanupOnceAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableServiceProviderCaching(false)
            .Options;
        return new AppDbContext(options);
    }

    private static TokenCleanupWorker CreateSut(IAppDbContext db)
    {
        var providerMock = new Mock<IServiceProvider>();
        providerMock.Setup(p => p.GetService(typeof(IAppDbContext))).Returns(db);

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(s => s.ServiceProvider).Returns(providerMock.Object);

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

        return new TokenCleanupWorker(
            scopeFactoryMock.Object,
            Options.Create(new TokenCleanupSetting { IntervalMinutes = 60 }),
            NullLogger<TokenCleanupWorker>.Instance
        );
    }

    private static async Task<int> RunCleanupAsync(TokenCleanupWorker worker, DateTimeOffset now)
    {
        return await (Task<int>)
            CleanupOnceAsyncMethod.Invoke(worker, new object[] { now, CancellationToken.None })!;
    }

    private static RefreshToken NewToken(DateTimeOffset? expiresAt = null, DateTimeOffset? deletedAt = null) =>
        new()
        {
            Token = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid(),
            DeviceFingerprint = "device-1",
            ExpiresAt = expiresAt ?? DateTimeOffset.UtcNow.AddDays(1),
            DeletedAt = deletedAt,
        };

    /// <summary>
    /// A refresh token that has expired is physically removed from the database.
    /// </summary>
    [Fact]
    public async Task CleanupOnceAsync_ExpiredToken_DeletesIt()
    {
        // Arrange
        var db = CreateInMemoryContext();
        var expired = NewToken(expiresAt: DateTimeOffset.UtcNow.AddMinutes(-1));
        db.RefreshTokens.Add(expired);
        await db.SaveChangesAsync();
        var worker = CreateSut(db);

        // Act
        var deleted = await RunCleanupAsync(worker, DateTimeOffset.UtcNow);

        // Assert
        deleted.Should().Be(1);
        db.RefreshTokens.IgnoreQueryFilters().Count(t => t.Id == expired.Id).Should().Be(0);
    }

    /// <summary>
    /// A refresh token that was soft-deleted (rotated/revoked) is physically removed.
    /// </summary>
    [Fact]
    public async Task CleanupOnceAsync_SoftDeletedToken_DeletesIt()
    {
        // Arrange
        var db = CreateInMemoryContext();
        var revoked = NewToken(deletedAt: DateTimeOffset.UtcNow);
        db.RefreshTokens.Add(revoked);
        await db.SaveChangesAsync();
        var worker = CreateSut(db);

        // Act
        var deleted = await RunCleanupAsync(worker, DateTimeOffset.UtcNow);

        // Assert
        deleted.Should().Be(1);
        db.RefreshTokens.IgnoreQueryFilters().Count(t => t.Id == revoked.Id).Should().Be(0);
    }

    /// <summary>
    /// A refresh token that is neither expired nor soft-deleted is left untouched.
    /// </summary>
    [Fact]
    public async Task CleanupOnceAsync_ActiveToken_KeepsIt()
    {
        // Arrange
        var db = CreateInMemoryContext();
        var active = NewToken();
        db.RefreshTokens.Add(active);
        await db.SaveChangesAsync();
        var worker = CreateSut(db);

        // Act
        var deleted = await RunCleanupAsync(worker, DateTimeOffset.UtcNow);

        // Assert
        deleted.Should().Be(0);
        db.RefreshTokens.IgnoreQueryFilters().Count(t => t.Id == active.Id).Should().Be(1);
    }

    /// <summary>
    /// Only stale tokens are removed while valid ones are preserved in the same pass.
    /// </summary>
    [Fact]
    public async Task CleanupOnceAsync_StaleAndActiveTokens_DeletesOnlyStale()
    {
        // Arrange
        var db = CreateInMemoryContext();
        var expired = NewToken(expiresAt: DateTimeOffset.UtcNow.AddMinutes(-5));
        var revoked = NewToken(deletedAt: DateTimeOffset.UtcNow);
        var active = NewToken();
        db.RefreshTokens.AddRange(expired, revoked, active);
        await db.SaveChangesAsync();
        var worker = CreateSut(db);

        // Act
        var deleted = await RunCleanupAsync(worker, DateTimeOffset.UtcNow);

        // Assert
        deleted.Should().Be(2);
        db.RefreshTokens.IgnoreQueryFilters().Should().OnlyContain(t => t.Id == active.Id);
    }

    /// <summary>
    /// A run with no stale tokens deletes nothing and completes without error.
    /// </summary>
    [Fact]
    public async Task CleanupOnceAsync_NoStaleTokens_DeletesNothing()
    {
        // Arrange
        var db = CreateInMemoryContext();
        var worker = CreateSut(db);

        // Act
        var deleted = await RunCleanupAsync(worker, DateTimeOffset.UtcNow);

        // Assert
        deleted.Should().Be(0);
        db.RefreshTokens.IgnoreQueryFilters().Should().BeEmpty();
    }
}