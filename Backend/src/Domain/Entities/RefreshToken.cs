using MyTarotReader.Domain.Common;

namespace MyTarotReader.Domain.Entities;

/// <summary>
/// A server-side refresh token issued to a device/session. One row is created per login and
/// per refresh (rotation), so a single user can hold multiple active tokens across devices.
/// Revocation is tracked via the inherited <see cref="BaseEntity.DeletedAt"/> soft-delete
/// marker (set on logout or rotation); a token whose <see cref="BaseEntity.DeletedAt"/> is
/// not null is rejected on use.
/// </summary>
public class RefreshToken : BaseEntity
{
    /// <summary>The random opaque token value sent to the client in an HttpOnly cookie.</summary>
    public string Token { get; set; } = null!;

    /// <summary>The id of the user this refresh token belongs to.</summary>
    public Guid UserId { get; set; }

    /// <summary>The UTC instant after which this token can no longer be used.</summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>Stable per-device id (frontend FingerprintJS visitorId) used to reject tokens
    /// reused on a different device. Bound at issuance and checked on refresh.</summary>
    public string DeviceFingerprint { get; set; } = null!;

    #region Navigation properties

    /// <summary>The user this refresh token belongs to.</summary>
    public User User { get; set; } = null!;

    #endregion
}
