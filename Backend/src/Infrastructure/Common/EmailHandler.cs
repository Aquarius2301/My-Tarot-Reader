using MyTarotReader.Application.Contracts.Common;

namespace MyTarotReader.Infrastructure.Common;

public class EmailHandler(IEmailSender emailSender, IEmailTemplateEngine templateEngine)
    : IEmailHandler
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

        var body = templateEngine.Render(
            resource,
            new Dictionary<string, string> { ["UserName"] = toName }
        );

        await emailSender.SendAsync(toEmail, toName, subject, body, cancellationToken);
    }
}
