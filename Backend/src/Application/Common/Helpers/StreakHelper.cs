using MyTarotReader.Domain.Entities;

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

    /// <summary>
    /// Normalizes the stored cycle day for display: a cycle runs from 0 to
    /// <paramref name="cycleDays"/> - 1, so a value equal to the cycle length is
    /// shown as 0.
    /// </summary>
    public static int GetCycleDayToShow(int cycleDay, int cycleDays) => cycleDay % cycleDays;

    /// <summary>
    /// Computes the next cycle day after a consecutive check-in. When the cycle
    /// would complete, it restarts from 1 instead.
    /// </summary>
    public static int GetNextCycleDay(int cycleDay, int cycleDays)
    {
        var next = cycleDay + 1;
        return next >= cycleDays ? 1 : next;
    }

    /// <summary>
    /// Resets the streak saver flag when a new calendar month (Vietnam time) has
    /// started since the last check-in.
    /// </summary>
    /// <param name="streak">The streak to update in place.</param>
    /// <param name="vietnamNow">The current Vietnam date and time.</param>
    /// <returns><c>true</c> when the saver flag was reset; otherwise <c>false</c>.</returns>
    public static bool ResetSaverIfNewMonth(Streak streak, DateTime vietnamNow)
    {
        if (!streak.IsSaverUsed || !IsNewMonth(streak.LastCheckIn, vietnamNow))
        {
            return false;
        }

        streak.IsSaverUsed = false;
        return true;
    }
}
