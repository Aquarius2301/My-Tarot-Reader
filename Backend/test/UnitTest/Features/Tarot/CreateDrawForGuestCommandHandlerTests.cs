using FluentAssertions;
using Moq;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Constants.Tarot;
using MyTarotReader.Application.Features.Tarot;
using StackExchange.Redis;
using Xunit;

namespace MyTarotReader.UnitTest.Features.Tarot;

public class CreateDrawForGuestCommandHandlerTests
{
    private const string ValidCard = "maj-00";

    private static (CreateDrawForGuestCommandHandler Sut, Mock<IDatabase> Db) CreateSut()
    {
        var redis = new Mock<IConnectionMultiplexer>();
        var redisDb = new Mock<IDatabase>();
        redis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(redisDb.Object);
        return (new CreateDrawForGuestCommandHandler(redis.Object), redisDb);
    }

    [Fact]
    public async Task CreateDrawForGuest_ValidKey_CreatesInRedis()
    {
        // Arrange
        var (sut, dbMock) = CreateSut();
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

        // Act
        await sut.Handle(new CreateDrawForGuestCommand(ValidCard, true, "guest-1"), CancellationToken.None);

        // Assert
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
        dbMock.Verify(
            d =>
                d.StringIncrementAsync(
                    (RedisKey)"tarot:stats:guest:total",
                    It.IsAny<long>(),
                    It.IsAny<CommandFlags>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateDrawForGuest_AlreadyDrawn_ThrowsTooMany()
    {
        // Arrange
        var (sut, dbMock) = CreateSut();
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

        // Act
        var act = () =>
            sut.Handle(new CreateDrawForGuestCommand(ValidCard, false, "guest-1"), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<TooManyRequestsException>();
        ex.Which.ErrorCode.Should().Be(TarotReadingErrorCode.DrawnAlready);
        dbMock.Verify(
            d =>
                d.StringIncrementAsync(
                    It.IsAny<RedisKey>(),
                    It.IsAny<long>(),
                    It.IsAny<CommandFlags>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task CreateDrawForGuest_AfterCooldown_AllowsAgain()
    {
        // Arrange
        var (sut, dbMock) = CreateSut();
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

        await sut.Handle(new CreateDrawForGuestCommand(ValidCard, false, "guest-1"), CancellationToken.None);

        // Act
        var act = () =>
            sut.Handle(new CreateDrawForGuestCommand(ValidCard, true, "guest-1"), CancellationToken.None);

        // Assert
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
}
