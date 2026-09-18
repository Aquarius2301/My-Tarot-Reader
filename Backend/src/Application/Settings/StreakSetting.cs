namespace MyTarotReader.Application.Settings;

/// <summary>
/// Configuration for the daily streak check-in.
/// Bound from the <c>Streak</c> appsettings section.
/// </summary>
public class StreakSetting
{
    /// <summary>Number of consecutive check-in days that complete one cycle.</summary>
    public int CycleDays { get; set; } = 7;

    /// <summary>
    /// White coins rewarded per consecutive check-in day. The reward is indexed by
    /// <c>(CurrentStreak - 1) % Count</c>, so the schedule repeats every cycle.
    /// </summary>
    public List<int> DailyCheckInRewards { get; set; } = [1, 1, 1, 1, 2, 2, 3];
}