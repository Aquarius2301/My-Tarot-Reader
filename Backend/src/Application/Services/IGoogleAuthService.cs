namespace MyTarotReader.Application.Services;

public record GooglePayload(string ProviderKey, string Email, string Name, string Picture);

public interface IGoogleAuthService
{
    Task<GooglePayload?> ValidateAsync(string credential);
}
