using FluentAssertions;
using Moq;
using MyTarotReader.Application.Features.Tarot;
using StackExchange.Redis;
using Xunit;

namespace MyTarotReader.UnitTest.Features.Tarot;

public class RemoveDrawnForGuestCommandHandlerTests
{
    private static (RemoveDrawnForGuestCommandHandler Sut, Mock<IDatabase> Db) CreateSut()
    {
        var redis = new Mock<IConnectionMultiplexer>();
        var redisDb = new Mock<IDatabase>();
        redis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(redisDb.Object);
        return (new RemoveDrawnForGuestCommandHandler(redis.Object), redisDb);
    }

    [Fact]
    public async Task RemoveDrawForGuest_ValidKey_Deletes()
    {
        // Arrange
        var (sut, dbMock) = CreateSut();
        dbMock
            .Setup(d => d.KeyDeleteAsync((RedisKey)"tarot:draw:guest-1", It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        await sut.Handle(new RemoveDrawnForGuestCommand("guest-1"), CancellationToken.None);

        // Assert
        dbMock.Verify(
            d => d.KeyDeleteAsync((RedisKey)"tarot:draw:guest-1", It.IsAny<CommandFlags>()),
            Times.Once
        );
    }

    [Fact]
    public async Task RemoveDrawForGuest_NoDraw_NoThrow()
    {
        // Arrange
        var (sut, dbMock) = CreateSut();
        dbMock
            .Setup(d => d.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        // Act
        var act = () => sut.Handle(new RemoveDrawnForGuestCommand("guest-1"), CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
