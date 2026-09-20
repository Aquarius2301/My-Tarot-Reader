using System.Threading.Channels;
using MyTarotReader.Application.Contracts.Backgrounds;

namespace MyTarotReader.Infrastructure.Backgrounds;

/// <summary>
/// An in-memory, unbounded channel that buffers email messages for the background worker.
/// </summary>
public class EmailBackgroundQueue : IEmailBackgroundQueue
{
    private readonly Channel<WelcomeEmailMessage> _channel = Channel.CreateUnbounded<
        WelcomeEmailMessage
    >();

    /// <inheritdoc />
    public async ValueTask EnqueueAsync(
        WelcomeEmailMessage message,
        CancellationToken cancellationToken = default
    )
    {
        await _channel.Writer.WriteAsync(message, cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask<WelcomeEmailMessage> DequeueAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _channel.Reader.ReadAsync(cancellationToken);
    }
}