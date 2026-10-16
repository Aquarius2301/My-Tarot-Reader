using Google.Apis.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Contracts.Common;
using MyTarotReader.Application.Settings;

namespace MyTarotReader.Infrastructure.Common;

public class GoogleAuthValidator(
    IOptions<GoogleSetting> googleSetting,
    ILogger<GoogleAuthValidator> logger
) : IGoogleAuthValidator
{
    private readonly GoogleSetting _googleSetting = googleSetting.Value;
    private readonly ILogger<GoogleAuthValidator> _logger = logger;

    public async Task<GooglePayload?> ValidateAsync(
        string credential,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_googleSetting.ClientId],
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(credential, settings);

            return new GooglePayload(payload.Subject, payload.Email, payload.Name, payload.Picture);
        }
        catch (InvalidJwtException ex)
        {
            // Invalid token or expired token
            _logger.LogWarning(ex, "Failed to verify Google Token: {Message}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify Google Token.");
            return null;
        }
    }
}
