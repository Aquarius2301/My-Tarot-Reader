using FluentAssertions;
using MyTarotReader.Application.Contracts.Backgrounds;
using MyTarotReader.Infrastructure.Backgrounds;
using Xunit;

namespace MyTarotReader.UnitTest.Backgrounds;

/// <summary>
/// Unit tests for <see cref="EmailBackgroundQueue"/>.
/// </summary>
public class EmailBackgroundQueueTests
{
    /// <summary>
    /// Messages are returned in the order they were enqueued (FIFO).
    /// </summary>
    [Fact]
    public async Task EnqueueAsync_ThenDequeueAsync_ReturnsMessagesInFifoOrder()
    {
        // Arrange
        var sut = new EmailBackgroundQueue();
        var first = new WelcomeEmailMessage("a@example.com", "A", "en");
        var second = new WelcomeEmailMessage("b@example.com", "B", "vi");

        // Act
        await sut.EnqueueAsync(first, CancellationToken.None);
        await sut.EnqueueAsync(second, CancellationToken.None);
        var dequeuedFirst = await sut.DequeueAsync(CancellationToken.None);
        var dequeuedSecond = await sut.DequeueAsync(CancellationToken.None);

        // Assert
        dequeuedFirst.Should().Be(first);
        dequeuedSecond.Should().Be(second);
    }

    /// <summary>
    /// Dequeueing from an empty queue waits until a message becomes available.
    /// </summary>
    [Fact]
    public async Task DequeueAsync_EmptyQueue_WaitsForNextMessage()
    {
        // Arrange
        var sut = new EmailBackgroundQueue();
        var message = new WelcomeEmailMessage("a@example.com", "A", "vi");

        // Act
        var dequeueTask = sut.DequeueAsync(CancellationToken.None).AsTask();
        await sut.EnqueueAsync(message, CancellationToken.None);
        var dequeued = await dequeueTask;

        // Assert
        dequeued.Should().Be(message);
    }
}