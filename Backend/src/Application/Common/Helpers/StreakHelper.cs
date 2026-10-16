namespace MyTarotReader.Application.Common.Helpers;

/// <summary>
/// Time helpers for the streak check-in feature, based on Vietnam time (UTC+7).
/// </summary>
public static class StreakHelper
{
    private static readonly TimeSpan VietnamOffset = TimeSpan.FromHours(7);

    /// <summary>
    /// Gets the current date and time in Vietnam (UTC+7, no daylight saving time).
    /// </summary>
    public static DateTime GetVietnamNow() => DateTime.UtcNow + VietnamOffset;

    /// <summary>
    /// Determines whether two date-times fall on the same calendar day.
    /// </summary>
    public static bool IsSameDay(DateTime a, DateTime b) => a.Date == b.Date;

    /// <summary>
    /// Determines whether the last check-in was the previous day
    /// relative to the given current time (consecutive day).
    /// </summary>
    public static bool IsConsecutiveDay(DateTime lastCheckIn, DateTime now)
    {
        var last = lastCheckIn.Date;
        var current = now.Date;
        return last == current.AddDays(-1);
    }

    /// <summary>
    /// Determines whether the streak is broken, i.e. the user missed more than
    /// one day since their last check-in and the streak must be reset.
    /// </summary>
    public static bool IsBroken(DateTime lastCheckIn, DateTime now)
    {
        var last = lastCheckIn.Date;
        var current = now.Date;
        return current > last.AddDays(1);
    }

    /// <summary>
    /// Determines whether the given current time is in a later calendar month
    /// than the last check-in, used to reset the streak saver.
    /// </summary>
    public static bool IsNewMonth(DateTime lastCheckIn, DateTime now)
    {
        var last = lastCheckIn.Date;
        var current = now.Date;
        return current.Year > last.Year
            || (current.Year == last.Year && current.Month > last.Month);
    }
}
