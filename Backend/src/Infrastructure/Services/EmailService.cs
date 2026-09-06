using System.Reflection;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Application.Exceptions;
using MyTarotReader.Application.Settings;

namespace MyTarotReader.Infrastructure.Services;

/// <inheritdoc />
/// <remarks>
/// Flow:
/// 1. Validate the recipient address.
/// 2. Build the welcome <see cref="MimeMessage"/> in the requested language (vi/en).
/// 3. Connect to SMTP, authenticate, send, and disconnect.
/// Any transport failure is surfaced as <see cref="InternalServerException"/>.
/// </remarks>
public class EmailService(IOptions<EmailSetting> emailSetting) : IEmailService
{
    private readonly EmailSetting _emailSetting = emailSetting.Value;

    /// <summary>
    /// Fully-qualified name of the embedded Vietnamese welcome HTML template.
    /// </summary>
    private const string WelcomeTemplateViResource =
        "MyTarotReader.Infrastructure.Templates.Welcome.WelcomeVi.html";

    /// <summary>
    /// Fully-qualified name of the embedded English welcome HTML template.
    /// </summary>
    private const string WelcomeTemplateEnResource =
        "MyTarotReader.Infrastructure.Templates.Welcome.WelcomeEn.html";

    /// <summary>
    /// Cached Vietnamese welcome HTML template, rendered per recipient by replacing {{UserName}}.
    /// </summary>
    private static readonly string WelcomeTemplateVi = LoadTemplate(WelcomeTemplateViResource);

    /// <summary>
    /// Cached English welcome HTML template, rendered per recipient by replacing {{UserName}}.
    /// </summary>
    private static readonly string WelcomeTemplateEn = LoadTemplate(WelcomeTemplateEnResource);

    /// <inheritdoc />
    public async Task SendWelcomeEmailAsync(
        string toEmail,
        string toName,
        string? language,
        CancellationToken cancellationToken = default
    )
    {
        // 1. Validate the recipient address.
        if (!MailboxAddress.TryParse(toEmail, out _))
            throw new BadRequestException(ErrorMessageCode.Email.InvalidAddress);

        // 2. Build the welcome message in the resolved language.
        var isVietnamese = ResolveVietnamese(language);
        var message = new MimeMessage
        {
            From = { new MailboxAddress(_emailSetting.FromName, _emailSetting.FromAddress) },
            To = { new MailboxAddress(toName, toEmail) },
            Subject = isVietnamese
                ? "Chào mừng đến My Tarot Reader"
                : "Welcome to My Tarot Reader",
            Body = new TextPart("html")
            {
                Text = isVietnamese
                    ? WelcomeTemplateVi.Replace("{{UserName}}", toName)
                    : WelcomeTemplateEn.Replace("{{UserName}}", toName),
            },
        };

        // 3. Deliver via SMTP.
        try
        {
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                _emailSetting.Host,
                _emailSetting.Port,
                ResolveSecureSocketOptions(),
                cancellationToken
            );
            if (!string.IsNullOrEmpty(_emailSetting.Username))
            {
                await smtp.AuthenticateAsync(
                    _emailSetting.Username,
                    _emailSetting.Password,
                    cancellationToken
                );
            }
            await smtp.SendAsync(message, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new InternalServerException(ErrorMessageCode.Email.SendFailed);
        }
    }

    /// <summary>
    /// Decides whether to use Vietnamese. Anything starting with "vi" is Vietnamese;
    /// otherwise the app default (Vietnamese) is used.
    /// </summary>
    private static bool ResolveVietnamese(string? language) =>
        string.IsNullOrWhiteSpace(language) || language.StartsWith("vi", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Loads an embedded HTML template from this assembly, caching it in a static field.
    /// </summary>
    /// <param name="resourceName">Fully-qualified manifest resource name of the template.</param>
    /// <returns>The template file contents.</returns>
    /// <exception cref="InvalidOperationException">
    /// The resource is not embedded in the assembly (packaging error).
    /// </exception>
    private static string LoadTemplate(string resourceName)
    {
        using var stream = typeof(EmailService).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Embedded resource '{resourceName}' was not found. "
                + "Ensure the .html file is present in Infrastructure/Templates and built as an EmbeddedResource.");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Picks the SMTP security mode from configuration: implicit TLS on port 465,
    /// STARTTLS otherwise, or plain when TLS is disabled.
    /// </summary>
    private SecureSocketOptions ResolveSecureSocketOptions() =>
        _emailSetting.EnableSsl switch
        {
            true when _emailSetting.Port == 465 => SecureSocketOptions.SslOnConnect,
            true => SecureSocketOptions.StartTls,
            _ => SecureSocketOptions.None,
        };
}