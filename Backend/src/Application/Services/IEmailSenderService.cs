namespace MyTarotReader.Application.Services;

public interface IEmailSenderService
{
    Task SendAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default
    );
}
