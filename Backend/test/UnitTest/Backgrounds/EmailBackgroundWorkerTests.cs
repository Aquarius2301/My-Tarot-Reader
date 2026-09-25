using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MyTarotReader.Application.Contracts.Backgrounds;
using MyTarotReader.Application.Contracts.Common;
using MyTarotReader.Infrastructure.Backgrounds;
using System.Reflection;
using Xunit;

namespace MyTarotReader.UnitTest.Backgrounds;

/// <summary>
/// Unit tests for <see cref="EmailBackgroundWorker"/>.
/// </summary>
public class EmailBackgroundWorkerTests
{
    private static readonly MethodInfo ExecuteAsyncMethod = typeof(EmailBackgroundWorker)
        .GetMethod("ExecuteAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static (
        EmailBackgroundWorker Worker,
        Mock<IEmailHandler> Email,
        Mock<IServiceScopeFactory> ScopeFactory,
        EmailBackgroundQueue Queue
    ) CreateSut()
    {
        var queue = new EmailBackgroundQueue();

        var email = new Mock<IEmailHandler>();
        email
            .Setup(e =>
                e.SendWelcomeEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);

        var providerMock = new Mock<IServiceProvider>();
        providerMock
            .Setup(p => p.GetService(typeof(IEmailHandler)))
            .Returns(email.Object);

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(s => s.ServiceProvider).Returns(providerMock.Object);

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

        var worker = new EmailBackgroundWorker(
            scopeFactoryMock.Object,
            queue,
            NullLogger<EmailBackgroundWorker>.Instance
        );

        return (worker, email, scopeFactoryMock, queue);
    }

    /// <summary>
    /// A queued message is sent through its own fresh DI scope, so the email is
    /// delivered independently of the HTTP request that enqueued it.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_MessageQueued_SendsWelcomeEmailInOwnScope()
    {
        // Arrange
        var (worker, email, scopeFactory, queue) = CreateSut();
        using var cts = new CancellationTokenSource();

        email
            .Setup(e =>
                e.SendWelcomeEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Callback(() => cts.Cancel())
            .Returns(Task.CompletedTask);

        await queue.EnqueueAsync(
            new WelcomeEmailMessage("a@example.com", "A", "en"),
            CancellationToken.None
        );

        // Act
        await ((Task)ExecuteAsyncMethod.Invoke(worker, new object[] { cts.Token })!);

        // Assert
        scopeFactory.Verify(f => f.CreateScope(), Times.Once);
        email.Verify(
            e =>
                e.SendWelcomeEmailAsync(
                    "a@example.com",
                    "A",
                    "en",
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    /// <summary>
    /// A failed send is logged and the worker keeps processing the next message.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_SendFails_KeepsProcessingNextMessage()
    {
        // Arrange
        var (worker, email, _, queue) = CreateSut();
        using var cts = new CancellationTokenSource();

        email
            .Setup(e =>
                e.SendWelcomeEmailAsync(
                    "bad@example.com",
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(new InvalidOperationException("smtp down"));
        email
            .Setup(e =>
                e.SendWelcomeEmailAsync(
                    "good@example.com",
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Callback(() => cts.Cancel())
            .Returns(Task.CompletedTask);

        await queue.EnqueueAsync(
            new WelcomeEmailMessage("bad@example.com", "Bad", "vi"),
            CancellationToken.None
        );
        await queue.EnqueueAsync(
            new WelcomeEmailMessage("good@example.com", "Good", "vi"),
            CancellationToken.None
        );

        // Act
        await ((Task)ExecuteAsyncMethod.Invoke(worker, new object[] { cts.Token })!);

        // Assert
        email.Verify(
            e =>
                e.SendWelcomeEmailAsync(
                    "bad@example.com",
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        email.Verify(
            e =>
                e.SendWelcomeEmailAsync(
                    "good@example.com",
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}