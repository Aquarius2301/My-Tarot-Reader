using FluentAssertions;
using Moq;
using MyTarotReader.Application.Features.Tarot;
using StackExchange.Redis;
using Xunit;

namespace MyTarotReader.UnitTest.Features.Tarot;

public class GetLastDrawnCardForGuestQueryHandlerTests
{
    private const string ValidCard = "maj-00";

    private static (GetLastDrawnCardForGuestQueryHandler Sut, Mock<IDatabase> Db) CreateSut()
    {
        var redis = new Mock<IConnectionMultiplexer>();
        var redisDb = new Mock<IDatabase>();
        redis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(redisDb.Object);
        return (new GetLastDrawnCardForGuestQueryHandler(redis.Object), redisDb);
    }

    [Fact]
    public async Task GetLastDrawnCardForGuest_HasDraw_ReturnsCardAndRemaining()
    {
        // Arrange
        var (sut, dbMock) = CreateSut();
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

        // Act
        var result = await sut.Handle(new GetLastDrawnCardForGuestQuery("guest-1"), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.CardCode.Should().Be(ValidCard);
        result.IsReversed.Should().BeTrue();
        result.RemainingSeconds.Should().Be((long)remaining.TotalSeconds);
    }

    [Fact]
    public async Task GetLastDrawnCardForGuest_NoDraw_ReturnsNull()
    {
        // Arrange
        var (sut, dbMock) = CreateSut();
        dbMock
            .Setup(d => d.StringGetWithExpiryAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValueWithExpiry(RedisValue.Null, null));

        // Act
        var result = await sut.Handle(new GetLastDrawnCardForGuestQuery("guest-1"), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
