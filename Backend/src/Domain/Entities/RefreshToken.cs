using MyTarotReader.Domain.Common;

namespace MyTarotReader.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = null!;

    public Guid UserId { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    /// Identifier of the device this token was issued to, used to scope refresh
    /// and prevent reuse across devices.
    /// </summary>
    public string DeviceFingerprint { get; set; } = null!;

    #region Navigation properties
    public User User { get; set; } = null!;

    #endregion
}
