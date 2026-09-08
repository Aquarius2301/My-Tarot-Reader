using MyTarotReader.Domain.Common;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Domain.Entities;

/// <summary>
/// Represents the Users table in the database.
/// Stores the authenticated account profile and role.
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// The user's display name.
    /// </summary>
    public string FullName { get; set; } = null!;

    /// <summary>
    /// The user's email address.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// The user's profile picture URL.
    /// </summary>
    public string Picture { get; set; } = null!;

    /// <summary>
    /// The provider subject used to correlate the external identity.
    /// </summary>
    public string ProviderKey { get; set; } = null!;

    /// <summary>
    /// The user's role.
    /// </summary>
    public UserRole Role { get; set; } = UserRole.Registered;

    #region Navigation Properties

    /// <summary>
    /// Refresh tokens issued for this user (one-to-many).
    /// </summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    /// <summary>
    /// Wallet associated with this user (one-to-one).
    /// </summary>
    public Wallet Wallet { get; set; } = null!;

    /// <summary>
    /// Read histories associated with this user (one-to-many).
    /// </summary>
    public List<ReadHistory> ReadHistories { get; set; } = [];

    /// <summary>
    /// AI read histories associated with this user (one-to-many).
    /// </summary>
    public List<AIReadHistory> AIReadHistories { get; set; } = [];

    /// <summary>
    /// AI chat histories associated with this user (one-to-many).
    /// </summary>
    public List<AIChatHistory> AIChatHistories { get; set; } = [];

    /// <summary>
    /// Transactions associated with this user (one-to-many).
    /// </summary>
    public List<Transaction> Transactions { get; set; } = [];
    #endregion
}
