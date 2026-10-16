using FluentAssertions;
using Moq;
using MyTarotReader.Application.Features.Admin;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.UnitTest.Common;
using StackExchange.Redis;
using Xunit;

namespace MyTarotReader.UnitTest.Features.Admin;

public class GetTarotStatisticQueryHandlerTests
{
    private static (GetTarotStatisticQueryHandler Sut, Mock<IDatabase> Db) CreateSut(AppDbContext db)
    {
        var redis = new Mock<IConnectionMultiplexer>();
        var redisDb = new Mock<IDatabase>();
        redis
            .Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(redisDb.Object);
        return (new GetTarotStatisticQueryHandler(db, redis.Object), redisDb);
    }

    private static void SeedUser(AppDbContext db, Guid userId)
    {
        db.Users.Add(
            new User
            {
                Id = userId,
                FullName = $"User-{userId:N}",
                Email = $"user-{userId:N}@example.com",
                Picture = "http://pic",
                ProviderKey = $"provider-{userId:N}",
                Role = UserRole.Registered,
            }
        );
        db.SaveChanges();
    }

    private static void SeedAiReading(
        AppDbContext db,
        Guid userId,
        string questionType,
        int cardCount
    )
    {
        db.AITarotReadings.Add(
            new AITarotReading
            {
                UserId = userId,
                Title = "Title",
                CardCount = cardCount,
                QuestionType = questionType,
                AnswerSummary = "Summary",
                Answer = "Answer",
                Cards = "[]",
                CreatedAt = DateTimeOffset.UtcNow,
            }
        );
        db.SaveChanges();
    }

    [Fact]
    public async Task GetTarotStatistic_WithData_ReturnsAggregatedCounts()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        SeedUser(db, userId);
        db.TarotReadings.Add(
            new TarotReading { UserId = userId, CardCode = "maj-00", IsReversed = false }
        );
        db.TarotReadings.Add(
            new TarotReading { UserId = userId, CardCode = "maj-01", IsReversed = true }
        );
        var deleted = new TarotReading { UserId = userId, CardCode = "maj-02", IsReversed = false };
        db.TarotReadings.Add(deleted);
        db.SaveChanges();
        deleted.DeletedAt = DateTimeOffset.UtcNow;
        db.SaveChanges();
        SeedAiReading(db, userId, "love", 3);
        SeedAiReading(db, userId, "love", 3);
        SeedAiReading(db, userId, "career", 5);
        var (sut, redisDb) = CreateSut(db);
        redisDb
            .Setup(d => d.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)42);

        // Act
        var result = await sut.Handle(new GetTarotStatisticQuery(), CancellationToken.None);

        // Assert
        result.TotalGuestTarotReadings.Should().Be(42);
        result.TotalRegisteredTarotReadings.Should().Be(2);
        result.TotalAiTarotReadings.Should().Be(3);
        result.AiTarotBreakdown.Should().HaveCount(2);
        result.AiTarotBreakdown[0]
            .Should()
            .BeEquivalentTo(new GetTarotStatisticAiTarotItem("love", 3, 2));
        result.AiTarotBreakdown[1]
            .Should()
            .BeEquivalentTo(new GetTarotStatisticAiTarotItem("career", 5, 1));
    }

    [Fact]
    public async Task GetTarotStatistic_NoData_ReturnsZerosAndEmptyBreakdown()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var (sut, redisDb) = CreateSut(db);
        redisDb
            .Setup(d => d.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        // Act
        var result = await sut.Handle(new GetTarotStatisticQuery(), CancellationToken.None);

        // Assert
        result.TotalGuestTarotReadings.Should().Be(0);
        result.TotalRegisteredTarotReadings.Should().Be(0);
        result.TotalAiTarotReadings.Should().Be(0);
        result.AiTarotBreakdown.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTarotStatistic_SoftDeletedReadings_AreExcluded()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        SeedUser(db, userId);
        var deleted = new TarotReading { UserId = userId, CardCode = "maj-00", IsReversed = false };
        db.TarotReadings.Add(deleted);
        db.SaveChanges();
        deleted.DeletedAt = DateTimeOffset.UtcNow;
        db.SaveChanges();
        var ai = new AITarotReading
        {
            UserId = userId,
            Title = "Title",
            CardCount = 3,
            QuestionType = "love",
            AnswerSummary = "Summary",
            Answer = "Answer",
            Cards = "[]",
        };
        db.AITarotReadings.Add(ai);
        db.SaveChanges();
        ai.DeletedAt = DateTimeOffset.UtcNow;
        db.SaveChanges();
        var (sut, redisDb) = CreateSut(db);
        redisDb
            .Setup(d => d.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        // Act
        var result = await sut.Handle(new GetTarotStatisticQuery(), CancellationToken.None);

        // Assert
        result.TotalRegisteredTarotReadings.Should().Be(0);
        result.TotalAiTarotReadings.Should().Be(0);
        result.AiTarotBreakdown.Should().BeEmpty();
    }
}
