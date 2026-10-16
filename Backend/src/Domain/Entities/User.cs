using MyTarotReader.Domain.Common;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Picture { get; set; } = null!;

    /// <summary>
    /// The unique key from the authentication provider (e.g. Google's "sub" claim),
    /// used to match returning users on login.
    /// </summary>
    public string ProviderKey { get; set; } = null!;

    public UserRole Role { get; set; } = UserRole.Registered;

    #region Navigation Properties

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];

    public Wallet Wallet { get; set; } = null!;

    public List<TarotReading> TarotReadings { get; set; } = [];

    public List<AITarotReading> AITarotReadings { get; set; } = [];

    public List<AIChatTarotReading> AIChatTarotReadings { get; set; } = [];

    public List<Order> Orders { get; set; } = [];
    #endregion
}
