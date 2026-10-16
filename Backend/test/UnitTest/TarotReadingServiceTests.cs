using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Common.Validators;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.Infrastructure.Services;
using StackExchange.Redis;
using Xunit;

namespace MyTarotReader.UnitTest;

/// <summary>
/// Unit tests for <see cref="TarotReadingService"/>. Authenticated-user paths run
/// against a real <see cref="AppDbContext"/> with the EF InMemory provider; guest
/// paths mock the Redis <see cref="IConnectionMultiplexer"/> / <see cref="IDatabase"/>
/// since guest draws live in Redis, not the database.
/// </summary>
public class TarotReadingServiceTests
{
    private const string ValidCard = "maj-00";
    private const string InvalidCard = "fake-card";

    #region Helpers

    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableServiceProviderCaching(false)
            .Options;
        return new AppDbContext(options);
    }

    // Guest-path SUT: in-memory DB is unused, Redis is mocked.
    private static (TarotReadingService Service, Mock<IDatabase> Db) CreateSut()
    {
        var db = CreateInMemoryContext();
        var redis = new Mock<IConnectionMultiplexer>();
        var redisDb = new Mock<IDatabase>();
        redis
            .Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(redisDb.Object);
        var service = new TarotReadingService(
            db,
            redis.Object,
            new CreateDrawForAuthRequestValidator(),
            new CreateDrawForGuestRequestValidator()
        );
        return (service, redisDb);
    }

    // Auth-path SUT: real DB + Redis mock (Redis never called in auth flows).
    private static (
        TarotReadingService Service,
        AppDbContext Db,
        Mock<IConnectionMultiplexer> Redis
    ) CreateAuthSut()
    {
        var db = CreateInMemoryContext();
        var redis = new Mock<IConnectionMultiplexer>();
        var service = new TarotReadingService(
            db,
            redis.Object,
            new CreateDrawForAuthRequestValidator(),
            new CreateDrawForGuestRequestValidator()
        );
        return (service, db, redis);
    }

    private static async Task SeedUserAsync(AppDbContext db, Guid userId)
    {
        var user = new User
        {
            Id = userId,
            FullName = $"User-{userId:N}",
            Email = $"user-{userId:N}@example.com",
            Picture = "http://pic",
            ProviderKey = $"provider-{userId:N}",
            Role = UserRole.Registered,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }

    #endregion

    #region CreateDrawForGuestAsync

    /// <summary>
    /// A valid guest key with a valid card stores the draw in Redis under
    /// <c>tarot:draw:{guestKey}</c> with a 12-hour TTL (When.NotExists), not in the DB.
    /// </summary>
    [Fact]
    public async Task CreateDrawForGuest_ValidKey_CreatesInRedis()
    {
        var (service, dbMock) = CreateSut();
        dbMock
            .Setup(d =>
                d.StringSetAsync(
                    It.IsAny<RedisKey>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<When>(),
                    It.IsAny<CommandFlags>()
                )
            )
            .ReturnsAsync(true);
        var request = new CreateDrawForGuestRequest(ValidCard, true);

        await service.CreateDrawForGuestAsync(request, "guest-1");

        dbMock.Verify(
            d =>
                d.StringSetAsync(
                    (RedisKey)"tarot:draw:guest-1",
                    It.IsAny<RedisValue>(),
                    It.Is<TimeSpan?>(t => t == TimeSpan.FromHours(12)),
                    When.NotExists,
                    It.IsAny<CommandFlags>()
                ),
            Times.Once
        );
        dbMock.Verify(
            d =>
                d.StringSetAsync(
                    It.IsAny<RedisKey>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<When>(),
                    It.IsAny<CommandFlags>()
                ),
            Times.Once
        );
    }

    /// <summary>
    /// When a draw already exists in Redis (StringSetAsync returns false), the guest is
    /// rate-limited with TooManyRequestsException using the "drawnAlready" code.
    /// </summary>
    [Fact]
    public async Task CreateDrawForGuest_AlreadyDrawn_ThrowsTooMany()
    {
        var (service, dbMock) = CreateSut();
        dbMock
            .Setup(d =>
                d.StringSetAsync(
                    It.IsAny<RedisKey>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<TimeSpan?>(),
                    When.NotExists,
                    It.IsAny<CommandFlags>()
                )
            )
            .ReturnsAsync(false);

        var request = new CreateDrawForGuestRequest(ValidCard, false);

        var act = async () => await service.CreateDrawForGuestAsync(request, "guest-1");

        await act.Should()
            .ThrowAsync<TooManyRequestsException>()
            .Where(e => e.ErrorCode == TarotReadingErrorCode.DrawnAlready);
    }

    /// <summary>
    /// A guest whose previous draw has expired out of the 12-hour TTL can draw again —
    /// the expired key no longer exists, so StringSetAsync succeeds once more.
    /// </summary>
    [Fact]
    public async Task CreateDrawForGuest_AfterCooldown_AllowsAgain()
    {
        var (service, dbMock) = CreateSut();
        // First draw succeeds...
        dbMock
            .Setup(d =>
                d.StringSetAsync(
                    It.IsAny<RedisKey>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<TimeSpan?>(),
                    When.NotExists,
                    It.IsAny<CommandFlags>()
                )
            )
            .ReturnsAsync(true);

        await service.CreateDrawForGuestAsync(
            new CreateDrawForGuestRequest(ValidCard, false),
            "guest-1"
        );

        // ...the expired key is gone, so a second draw is allowed too.
        var act = async () =>
            await service.CreateDrawForGuestAsync(
                new CreateDrawForGuestRequest(ValidCard, true),
                "guest-1"
            );

        await act.Should().NotThrowAsync();
        dbMock.Verify(
            d =>
                d.StringSetAsync(
                    It.IsAny<RedisKey>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<When>(),
                    It.IsAny<CommandFlags>()
                ),
            Times.Exactly(2)
        );
    }

    #endregion

    #region GetLastDrawnCardForGuestAsync

    /// <summary>
    /// A guest with a draw still within the cooldown window gets back the stored
    /// CardCode/IsReversed and a positive RemainingSeconds matching the TTL left.
    /// </summary>
    [Fact]
    public async Task GetLastDrawnCardForGuest_HasDraw_ReturnsCardAndRemaining()
    {
        var (service, dbMock) = CreateSut();
        var remaining = TimeSpan.FromHours(10);
        var payload = System.Text.Json.JsonSerializer.Serialize(
            new
            {
                DrawnAtUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                CardCode = ValidCard,
                IsReversed = true,
            }
        );
        dbMock
            .Setup(d =>
                d.StringGetWithExpiryAsync((RedisKey)"tarot:draw:guest-1", It.IsAny<CommandFlags>())
            )
            .ReturnsAsync(new RedisValueWithExpiry((RedisValue)payload, remaining));

        var result = await service.GetLastDrawnCardForGuestAsync("guest-1");

        result.Should().NotBeNull();
        result!.CardCode.Should().Be(ValidCard);
        result.IsReversed.Should().BeTrue();
        result.RemainingSeconds.Should().Be((long)remaining.TotalSeconds);
    }

    /// <summary>
    /// A guest with no draw (or an expired one) gets a null result.
    /// </summary>
    [Fact]
    public async Task GetLastDrawnCardForGuest_NoDraw_ReturnsNull()
    {
        var (service, dbMock) = CreateSut();
        dbMock
            .Setup(d => d.StringGetWithExpiryAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValueWithExpiry(RedisValue.Null, null));

        var result = await service.GetLastDrawnCardForGuestAsync("guest-1");

        result.Should().BeNull();
    }

    #endregion

    #region CreateDrawForAuthAsync

    /// <summary>
    /// A valid draw for an authenticated user is stored in the DB, tied to the correct
    /// userId with the requested card/reversed flags.
    /// </summary>
    [Fact]
    public async Task CreateDrawForAuth_Valid_StoresForUser()
    {
        var (service, db, _) = CreateAuthSut();
        var userId = Guid.NewGuid();

        await service.CreateDrawForAuthAsync(new CreateDrawForAuthRequest(ValidCard, true), userId);

        var entity = Assert.Single(db.TarotReadings);
        entity.UserId.Should().Be(userId);
        entity.CardCode.Should().Be(ValidCard);
        entity.IsReversed.Should().BeTrue();
    }

    #endregion

    #region GetLastDrawnCardForAuthAsync

    /// <summary>
    /// A user with draws gets the most recent one (newest CreatedAt), correctly mapped.
    /// </summary>
    [Fact]
    public async Task GetLastDrawnCardForAuth_HasDraw_ReturnsLatest()
    {
        var (service, db, _) = CreateAuthSut();
        var userId = Guid.NewGuid();
        await SeedUserAsync(db, userId);
        db.TarotReadings.Add(
            new TarotReading
            {
                UserId = userId,
                CardCode = "maj-01",
                IsReversed = false,
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-2),
            }
        );
        db.TarotReadings.Add(
            new TarotReading
            {
                UserId = userId,
                CardCode = ValidCard,
                IsReversed = true,
                CreatedAt = DateTimeOffset.UtcNow,
            }
        );
        await db.SaveChangesAsync();

        var result = await service.GetLastDrawnCardForAuthAsync(userId);

        result.Should().NotBeNull();
        result!.CardCode.Should().Be(ValidCard);
        result.IsReversed.Should().BeTrue();
    }

    /// <summary>
    /// A user with no draws gets null.
    /// </summary>
    [Fact]
    public async Task GetLastDrawnCardForAuth_NoDraw_ReturnsNull()
    {
        var (service, _, _) = CreateAuthSut();

        var result = await service.GetLastDrawnCardForAuthAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    /// <summary>
    /// Only the requested user's draws are considered; another user's new draw is ignored.
    /// </summary>
    [Fact]
    public async Task GetLastDrawnCardForAuth_OnlyOwnUserDraws()
    {
        var (service, db, _) = CreateAuthSut();
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        await SeedUserAsync(db, userA);
        await SeedUserAsync(db, userB);
        db.TarotReadings.Add(
            new TarotReading
            {
                UserId = userA,
                CardCode = ValidCard,
                IsReversed = false,
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            }
        );
        db.TarotReadings.Add(
            new TarotReading
            {
                UserId = userB,
                CardCode = "maj-21",
                IsReversed = true,
                CreatedAt = DateTimeOffset.UtcNow,
            }
        );
        await db.SaveChangesAsync();

        var result = await service.GetLastDrawnCardForAuthAsync(userA);

        result!.CardCode.Should().Be(ValidCard);
        result.IsReversed.Should().BeFalse();
    }

    #endregion

    #region RemoveDrawForGuestAsync

    /// <summary>
    /// Removing an existing guest draw deletes the Redis key.
    /// </summary>
    [Fact]
    public async Task RemoveDrawForGuest_ValidKey_Deletes()
    {
        var (service, dbMock) = CreateSut();
        dbMock
            .Setup(d => d.KeyDeleteAsync((RedisKey)"tarot:draw:guest-1", It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        await service.RemoveDrawForGuestAsync("guest-1");

        dbMock.Verify(
            d => d.KeyDeleteAsync((RedisKey)"tarot:draw:guest-1", It.IsAny<CommandFlags>()),
            Times.Once
        );
    }

    /// <summary>
    /// Removing a key that does not exist is a no-op (idempotent), no throw.
    /// </summary>
    [Fact]
    public async Task RemoveDrawForGuest_NoDraw_NoThrow()
    {
        var (service, dbMock) = CreateSut();
        dbMock
            .Setup(d => d.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        var act = async () => await service.RemoveDrawForGuestAsync("guest-1");

        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Validation

    /// <summary>
    /// An invalid card code is rejected before any draw is created — the service throws
    /// a generic <see cref="BadRequestException"/> whose ErrorCode becomes the response's
    /// Message (no field-level Data), and nothing is written to the DB.
    /// </summary>
    [Fact]
    public async Task CreateDrawForAuth_InvalidCard_ThrowsBadRequest()
    {
        var (service, db, _) = CreateAuthSut();
        var userId = Guid.NewGuid();

        var act = async () =>
            await service.CreateDrawForAuthAsync(
                new CreateDrawForAuthRequest(InvalidCard, true),
                userId
            );

        await act.Should()
            .ThrowAsync<BadRequestException>()
            .Where(e => e.ErrorCode == TarotReadingErrorCode.InvalidCardCode);
        db.TarotReadings.Should().BeEmpty();
    }

    /// <summary>
    /// An invalid card code for a guest draw is rejected before Redis is touched — the
    /// service throws a generic <see cref="BadRequestException"/> (error in Message), and
    /// StringSetAsync is never called.
    /// </summary>
    [Fact]
    public async Task CreateDrawForGuest_InvalidCard_ThrowsBadRequest()
    {
        var (service, dbMock) = CreateSut();

        var act = async () =>
            await service.CreateDrawForGuestAsync(
                new CreateDrawForGuestRequest(InvalidCard, false),
                "guest-1"
            );

        await act.Should()
            .ThrowAsync<BadRequestException>()
            .Where(e => e.ErrorCode == TarotReadingErrorCode.InvalidCardCode);
        dbMock.Verify(
            d =>
                d.StringSetAsync(
                    It.IsAny<RedisKey>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<When>(),
                    It.IsAny<CommandFlags>()
                ),
            Times.Never
        );
    }

    /// <summary>
    /// An empty/whitespace guest key (missing "X-Device-Id" header) is rejected before
    /// Redis is touched — the service throws a <see cref="BadRequestException"/> with the
    /// InvalidGuestKey code, so header-less clients cannot share a single draw slot.
    /// </summary>
    [Fact]
    public async Task CreateDrawForGuest_EmptyKey_ThrowsBadRequest()
    {
        var (service, dbMock) = CreateSut();

        var act = async () =>
            await service.CreateDrawForGuestAsync(
                new CreateDrawForGuestRequest(ValidCard, false),
                ""
            );

        await act.Should()
            .ThrowAsync<BadRequestException>()
            .Where(e => e.ErrorCode == TarotReadingErrorCode.InvalidGuestKey);
        dbMock.Verify(
            d =>
                d.StringSetAsync(
                    It.IsAny<RedisKey>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<When>(),
                    It.IsAny<CommandFlags>()
                ),
            Times.Never
        );
    }

    /// <summary>
    /// A whitespace-only guest key is treated the same as empty — rejected with
    /// InvalidGuestKey, Redis never touched.
    /// </summary>
    [Fact]
    public async Task CreateDrawForGuest_WhitespaceKey_ThrowsBadRequest()
    {
        var (service, dbMock) = CreateSut();

        var act = async () =>
            await service.CreateDrawForGuestAsync(
                new CreateDrawForGuestRequest(ValidCard, false),
                "   "
            );

        await act.Should()
            .ThrowAsync<BadRequestException>()
            .Where(e => e.ErrorCode == TarotReadingErrorCode.InvalidGuestKey);
        dbMock.Verify(
            d =>
                d.StringSetAsync(
                    It.IsAny<RedisKey>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<When>(),
                    It.IsAny<CommandFlags>()
                ),
            Times.Never
        );
    }

    /// <summary>
    /// Reading the last draw with an empty guest key is rejected with InvalidGuestKey.
    /// </summary>
    [Fact]
    public async Task GetLastDrawnCardForGuest_EmptyKey_ThrowsBadRequest()
    {
        var (service, dbMock) = CreateSut();

        var act = async () => await service.GetLastDrawnCardForGuestAsync("");

        await act.Should()
            .ThrowAsync<BadRequestException>()
            .Where(e => e.ErrorCode == TarotReadingErrorCode.InvalidGuestKey);
        dbMock.Verify(
            d => d.StringGetWithExpiryAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()),
            Times.Never
        );
    }

    /// <summary>
    /// Removing a draw with an empty guest key is rejected with InvalidGuestKey.
    /// </summary>
    [Fact]
    public async Task RemoveDrawForGuest_EmptyKey_ThrowsBadRequest()
    {
        var (service, dbMock) = CreateSut();

        var act = async () => await service.RemoveDrawForGuestAsync("");

        await act.Should()
            .ThrowAsync<BadRequestException>()
            .Where(e => e.ErrorCode == TarotReadingErrorCode.InvalidGuestKey);
        dbMock.Verify(
            d => d.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()),
            Times.Never
        );
    }

    #endregion

    #region GetAllReadingAsync

    /// <summary>
    /// A user with readings gets back the full list with every field mapped from the
    /// stored rows (Id, CardCode, IsReversed, CreatedAt).
    /// </summary>
    [Fact]
    public async Task GetAllReadingAsync_UserHasHistory_ReturnsMappedHistory()
    {
        var (service, db, _) = CreateAuthSut();
        var userId = Guid.NewGuid();
        await SeedUserAsync(db, userId);
        db.TarotReadings.Add(
            new TarotReading
            {
                UserId = userId,
                CardCode = ValidCard,
                IsReversed = false,
                CreatedAt = DateTimeOffset.UtcNow,
            }
        );
        db.TarotReadings.Add(
            new TarotReading
            {
                UserId = userId,
                CardCode = "min-wands-1",
                IsReversed = true,
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5),
            }
        );
        await db.SaveChangesAsync();

        var result = await service.GetAllReadingAsync(userId);

        result.Items.Should().HaveCount(2);
        var first = result.Items.Single(x => x.CardCode == ValidCard);
        first.CardCode.Should().Be(ValidCard);
        first.IsReversed.Should().BeFalse();

        var second = result.Items.Single(x => x.CardCode == "min-wands-1");
        second.IsReversed.Should().BeTrue();
    }

    /// <summary>
    /// A user with no readings gets back an empty (non-null) list.
    /// </summary>
    [Fact]
    public async Task GetAllReadingAsync_NoHistory_ReturnsEmptyList()
    {
        var (service, _, _) = CreateAuthSut();

        var result = await service.GetAllReadingAsync(Guid.NewGuid());

        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
    }

    /// <summary>
    /// Only readings belonging to the requested user are returned; another user's
    /// readings are never mixed in.
    /// </summary>
    [Fact]
    public async Task GetAllReadingAsync_OnlyReturnsUsersOwnHistory()
    {
        var (service, db, _) = CreateAuthSut();
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        await SeedUserAsync(db, userA);
        await SeedUserAsync(db, userB);
        db.TarotReadings.Add(
            new TarotReading
            {
                UserId = userA,
                CardCode = ValidCard,
                IsReversed = false,
                CreatedAt = DateTimeOffset.UtcNow,
            }
        );
        db.TarotReadings.Add(
            new TarotReading
            {
                UserId = userA,
                CardCode = "min-wands-1",
                IsReversed = true,
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            }
        );
        db.TarotReadings.Add(
            new TarotReading
            {
                UserId = userB,
                CardCode = "maj-10",
                IsReversed = false,
                CreatedAt = DateTimeOffset.UtcNow,
            }
        );
        await db.SaveChangesAsync();

        var result = await service.GetAllReadingAsync(userA);

        result.Items.Should().HaveCount(2);
        result
            .Items.Should()
            .AllSatisfy(x => x.CardCode.Should().BeOneOf(ValidCard, "min-wands-1"));
    }

    /// <summary>
    /// Readings are returned newest-first (ordered by CreatedAt descending).
    /// </summary>
    [Fact]
    public async Task GetAllReadingAsync_ReturnsOrderedByCreatedAtDescending()
    {
        var (service, db, _) = CreateAuthSut();
        var userId = Guid.NewGuid();
        await SeedUserAsync(db, userId);
        var oldest = DateTimeOffset.UtcNow.AddDays(-2);
        var middle = DateTimeOffset.UtcNow.AddDays(-1);
        var newest = DateTimeOffset.UtcNow;
        db.TarotReadings.Add(
            new TarotReading
            {
                UserId = userId,
                CardCode = "oldest",
                IsReversed = false,
                CreatedAt = oldest,
            }
        );
        db.TarotReadings.Add(
            new TarotReading
            {
                UserId = userId,
                CardCode = "middle",
                IsReversed = true,
                CreatedAt = middle,
            }
        );
        db.TarotReadings.Add(
            new TarotReading
            {
                UserId = userId,
                CardCode = "newest",
                IsReversed = false,
                CreatedAt = newest,
            }
        );
        await db.SaveChangesAsync();

        var result = await service.GetAllReadingAsync(userId);

        result.Items.Select(x => x.CreatedAt).Should().Equal(newest, middle, oldest);
    }

    /// <summary>
    /// Soft-deleted readings (DeletedAt set) are excluded by the global query filter.
    /// </summary>
    [Fact]
    public async Task GetAllReadingAsync_ExcludesSoftDeletedRecords()
    {
        var (service, db, _) = CreateAuthSut();
        var userId = Guid.NewGuid();
        await SeedUserAsync(db, userId);
        db.TarotReadings.Add(
            new TarotReading
            {
                UserId = userId,
                CardCode = "kept",
                IsReversed = false,
                CreatedAt = DateTimeOffset.UtcNow,
            }
        );
        var deleted = new TarotReading
        {
            UserId = userId,
            CardCode = "deleted",
            IsReversed = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        db.TarotReadings.Add(deleted);
        await db.SaveChangesAsync();

        deleted.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();

        var result = await service.GetAllReadingAsync(userId);

        result.Items.Should().HaveCount(1);
        result.Items.Single().CardCode.Should().Be("kept");
    }

    #endregion

    #region DeleteReadingAsync

    /// <summary>
    /// Deleting an existing reading soft-deletes it (DeletedAt set) and makes it
    /// disappear from further GetAllReadingAsync results.
    /// </summary>
    [Fact]
    public async Task DeleteReadingAsync_ExistingRecord_SoftDeletes()
    {
        var (service, db, _) = CreateAuthSut();
        var userId = Guid.NewGuid();
        await SeedUserAsync(db, userId);
        db.TarotReadings.Add(
            new TarotReading
            {
                UserId = userId,
                CardCode = ValidCard,
                IsReversed = false,
                CreatedAt = DateTimeOffset.UtcNow,
            }
        );
        await db.SaveChangesAsync();
        var reading = db.TarotReadings.Single();

        await service.DeleteReadingAsync(userId, reading.Id);

        db.TarotReadings.IgnoreQueryFilters()
            .Single(r => r.Id == reading.Id)
            .DeletedAt.Should()
            .NotBeNull();
        var afterDelete = await service.GetAllReadingAsync(userId);
        afterDelete.Items.Should().BeEmpty();
    }

    /// <summary>
    /// A reading id that does not exist throws NotFoundException with the
    /// TarotReadingErrorCode.NotFound code.
    /// </summary>
    [Fact]
    public async Task DeleteReadingAsync_ReadingIdNotExist_ThrowsNotFound()
    {
        var (service, _, _) = CreateAuthSut();
        var userId = Guid.NewGuid();

        var act = async () => await service.DeleteReadingAsync(userId, Guid.NewGuid());

        await act.Should()
            .ThrowAsync<NotFoundException>()
            .Where(e => e.ErrorCode == TarotReadingErrorCode.NotFound);
    }

    /// <summary>
    /// A reading that exists but belongs to a different user is treated as not found
    /// (NotFoundException) and the other user's reading is left untouched.
    /// </summary>
    [Fact]
    public async Task DeleteReadingAsync_RecordBelongsToOtherUser_ThrowsNotFound()
    {
        var (service, db, _) = CreateAuthSut();
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        await SeedUserAsync(db, userA);
        await SeedUserAsync(db, userB);
        db.TarotReadings.Add(
            new TarotReading
            {
                UserId = userB,
                CardCode = ValidCard,
                IsReversed = false,
                CreatedAt = DateTimeOffset.UtcNow,
            }
        );
        await db.SaveChangesAsync();
        var readingB = db.TarotReadings.Single();

        var act = async () => await service.DeleteReadingAsync(userA, readingB.Id);

        await act.Should()
            .ThrowAsync<NotFoundException>()
            .Where(e => e.ErrorCode == TarotReadingErrorCode.NotFound);
        db.TarotReadings.IgnoreQueryFilters()
            .Single(r => r.Id == readingB.Id)
            .DeletedAt.Should()
            .BeNull();
    }

    #endregion
}
