namespace MyTarotReader.Application.Services;

public interface IEmailHandlerService
{
    Task SendWelcomeEmailAsync(
        string toEmail,
        string toName,
        string? language,
        CancellationToken cancellationToken = default
    );
}
