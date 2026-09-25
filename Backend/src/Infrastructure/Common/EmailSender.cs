using System.Net.Http.Json;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Contracts.Common;
using MyTarotReader.Application.Settings;

namespace MyTarotReader.Infrastructure.Common;

/// <summary>
/// Sends emails through the Resend HTTPS API using a raw HttpClient (no external SDK),
/// configured from <see cref="EmailSetting"/>.
/// </summary>
/// <remarks>
/// The HTTP API is used instead of SMTP because hosting providers such as Render block
/// outbound traffic to the SMTP ports 25, 465 and 587 on their free tier.
/// </remarks>
public class EmailSender(HttpClient httpClient, IOptions<EmailSetting> emailSetting) : IEmailSender
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly EmailSetting _emailSetting = emailSetting.Value;

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

        if (string.IsNullOrWhiteSpace(_emailSetting.ApiKey))
            throw new InternalServerException(EmailErrorCode.SendFailed);

        var from = string.IsNullOrWhiteSpace(_emailSetting.FromName)
            ? _emailSetting.FromAddress
            : $"{_emailSetting.FromName} <{_emailSetting.FromAddress}>";

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
                $"{_emailSetting.Endpoint.TrimEnd('/')}/emails"
            );
            httpRequest.Headers.Add("Authorization", $"Bearer {_emailSetting.ApiKey}");
            httpRequest.Content = JsonContent.Create(request);

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
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
