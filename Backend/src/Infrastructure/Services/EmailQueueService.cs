using System.Threading.Channels;
using MyTarotReader.Application.Services;

namespace MyTarotReader.Infrastructure.Services;

/// <summary>
/// An in-memory, unbounded channel that buffers email messages for the background worker.
/// </summary>
public class EmailQueueService : IEmailQueueService
{
    private readonly Channel<WelcomeEmailMessage> _channel =
        Channel.CreateUnbounded<WelcomeEmailMessage>();

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
