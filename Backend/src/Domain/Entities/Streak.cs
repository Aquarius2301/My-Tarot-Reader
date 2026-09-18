using MyTarotReader.Domain.Common;

namespace MyTarotReader.Domain.Entities;

public class Streak : BaseEntity
{
    public Guid UserId { get; set; }

    /// <summary>
    /// The number of consecutive days the user has checked in current.
    /// </summary>
    public int CurrentStreak { get; set; }

    /// <summary>
    /// The maximum number of consecutive days the user has checked in.
    /// </summary>
    public int LongestStreak { get; set; }

    /// <summary>
    /// The current day in the user's cycle, which resets after reaching a certain threshold (e.g., 7 days).
    /// </summary>
    public int CycleDay { get; set; }

    /// <summary>
    /// The date and time of the user's last check-in, used to determine if the streak should be incremented or reset.
    /// </summary>
    public DateTime LastCheckIn { get; set; }

    /// <summary>
    /// Indicates whether the user has used a "streak saver" feature, which allows them to maintain their streak even if they miss a day.
    /// </summary>
    public bool IsSaverUsed { get; set; } = false;

    #region Navigation Properties

    public User User { get; set; } = null!;

    #endregion
}
