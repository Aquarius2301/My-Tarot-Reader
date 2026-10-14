using MyTarotReader.Application.Services;

namespace MyTarotReader.Infrastructure.Services;

public class EmailHandlerService(
    IEmailSenderService emailSenderService,
    IEmailTemplateEngineService templateEngineService
) : IEmailHandlerService
{
    private const string WelcomeVi =
        "MyTarotReader.Infrastructure.Templates.Welcome.WelcomeVi.html";

    private const string WelcomeEn =
        "MyTarotReader.Infrastructure.Templates.Welcome.WelcomeEn.html";

    public async Task SendWelcomeEmailAsync(
        string toEmail,
        string toName,
        string? language,
        CancellationToken cancellationToken = default
    )
    {
        var isVi =
            string.IsNullOrWhiteSpace(language)
            || language.StartsWith("vi", StringComparison.OrdinalIgnoreCase);
        var resource = isVi ? WelcomeVi : WelcomeEn;
        var subject = isVi ? "Chào mừng đến My Tarot Reader" : "Welcome to My Tarot Reader";

        var body = templateEngineService.Render(
            resource,
            new Dictionary<string, string> { ["UserName"] = toName }
        );

        await emailSenderService.SendAsync(toEmail, toName, subject, body, cancellationToken);
    }
}
