using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Contracts.Common;
using MyTarotReader.Application.Settings;

namespace MyTarotReader.Infrastructure.Common;

public class EmailSender(IOptions<EmailSetting> emailSetting) : IEmailSender
{
    private readonly EmailSetting _emailSetting = emailSetting.Value;

    public async Task SendAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default
    )
    {
        if (!MailboxAddress.TryParse(toEmail, out _))
            throw new BadRequestException(EmailErrorCode.InvalidAddress);

        var message = new MimeMessage
        {
            From = { new MailboxAddress(_emailSetting.FromName, _emailSetting.FromAddress) },
            To = { new MailboxAddress(toName, toEmail) },
            Subject = subject,
            Body = new TextPart("html") { Text = htmlBody },
        };

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
        catch (Exception ex)
        {
            throw new InternalServerException(EmailErrorCode.SendFailed, innerException: ex);
        }
    }

    private SecureSocketOptions ResolveSecureSocketOptions() =>
        _emailSetting.EnableSsl switch
        {
            true when _emailSetting.Port == 465 => SecureSocketOptions.SslOnConnect,
            true => SecureSocketOptions.StartTls,
            _ => SecureSocketOptions.None,
        };
}
