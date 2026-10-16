using Google.Apis.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Services;
using MyTarotReader.Application.Settings;

namespace MyTarotReader.Infrastructure.Services;

public class GoogleAuthService(
    IOptions<GoogleSetting> googleSetting,
    ILogger<GoogleAuthService> logger
) : IGoogleAuthService
{
    public async Task<GooglePayload?> ValidateAsync(string credential)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [googleSetting.Value.ClientId],
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(credential, settings);

            return new GooglePayload(payload.Subject, payload.Email, payload.Name, payload.Picture);
        }
        catch (InvalidJwtException ex)
        {
            // Invalid token or expired token
            logger.LogWarning(ex, "Failed to verify Google Token: {Message}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to verify Google Token.");
            return null;
        }
    }
}
