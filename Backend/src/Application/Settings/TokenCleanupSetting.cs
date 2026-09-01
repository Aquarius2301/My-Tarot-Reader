namespace MyTarotReader.Application.Settings;

/// <summary>Configuration for the background refresh-token cleanup service.</summary>
public class TokenCleanupSetting
{
    /// <summary>
    /// How often the cleanup service physically deletes revoked and expired refresh tokens,
    /// in minutes. Bound from the <c>TokenCleanup:IntervalMinutes</c> appsettings section.
    /// </summary>
    public int IntervalMinutes { get; set; } = 60;
}
