using System.Net.Http.Json;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Services;
using MyTarotReader.Application.Settings;

namespace MyTarotReader.Infrastructure.Services;

/// <summary>
/// Sends emails through the Resend HTTPS API using a raw HttpClient (no external SDK),
/// configured from <see cref="EmailSetting"/>.
/// </summary>
/// <remarks>
/// The HTTP API is used instead of SMTP because hosting providers such as Render block
/// outbound traffic to the SMTP ports 25, 465 and 587 on their free tier.
/// </remarks>
public class EmailSenderService(HttpClient httpClient, IOptions<EmailSetting> emailSetting)
    : IEmailSenderService
{
    /// <inheritdoc />
    public async Task SendAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default
    )
    {
        if (!MailAddress.TryCreate(toEmail, out _))
            throw new BadRequestException(EmailErrorCode.InvalidAddress);

        if (string.IsNullOrWhiteSpace(emailSetting.Value.ApiKey))
            throw new InternalServerException(EmailErrorCode.SendFailed);

        var from = string.IsNullOrWhiteSpace(emailSetting.Value.FromName)
            ? emailSetting.Value.FromAddress
            : $"{emailSetting.Value.FromName} <{emailSetting.Value.FromAddress}>";

        var request = new
        {
            from,
            to = new[] { toEmail },
            subject,
            html = htmlBody,
        };

        try
        {
            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                $"{emailSetting.Value.Endpoint.TrimEnd('/')}/emails"
            );
            httpRequest.Headers.Add("Authorization", $"Bearer {emailSetting.Value.ApiKey}");
            httpRequest.Content = JsonContent.Create(request);

            using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException(
                    $"Resend API returned {(int)response.StatusCode}: {error}"
                );
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InternalServerException(EmailErrorCode.SendFailed, innerException: ex);
        }
    }
}
