namespace MyTarotReader.Application.Contracts.Services;

/// <summary>
/// Result of retrieving the current streak information for a user.
/// </summary>
/// <param name="CycleDay">The current day in the user's 7-day cycle (0-6).</param>
/// <param name="CurrentStreak">The number of consecutive check-in days.</param>
/// <param name="LongestStreak">The longest consecutive check-in count achieved.</param>
/// <param name="IsSaverUsed">Whether the user has already used the streak saver this month.</param>
/// <param name="IsCheckedInToday">Whether the user has already checked in today.</param>
public record GetStreakResult(
    int CycleDay,
    int CurrentStreak,
    int LongestStreak,
    bool IsSaverUsed,
    bool IsCheckedInToday
);

/// <summary>
/// Result of a successful check-in for a user.
/// </summary>
/// <param name="CycleDay">The current day in the user's 7-day cycle (0-6).</param>
/// <param name="CurrentStreak">The number of consecutive check-in days.</param>
/// <param name="LongestStreak">The longest consecutive check-in count achieved.</param>
/// <param name="IsSaverUsed">Whether the user has already used the streak saver this month.</param>
/// <param name="IsCheckedInToday">Always true after a check-in.</param>
public record CheckInResult(
    int CycleDay,
    int CurrentStreak,
    int LongestStreak,
    bool IsSaverUsed,
    bool IsCheckedInToday
);

public interface IStreakService
{
    /// <summary>
    /// Retrieves the current streak information for a user.
    /// </summary>
    /// <param name="userId">The authenticated user's ID.</param>
    /// <returns>
    /// <see cref="GetStreakResult"/> containing the cycle day, current/longest streak,
    /// saver status and whether the user checked in today. Returns default values
    /// when the user has no streak yet.
    /// </returns>
    /// <remarks>The streak saver is automatically reset at the start of a new month (Vietnam time).</remarks>
    Task<GetStreakResult> GetStreakAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Performs a daily check-in for a user, creating a new streak when none exists.
    /// </summary>
    /// <param name="userId">The authenticated user's ID.</param>
    /// <exception cref="BadRequestException">Thrown when the user has already checked in today.</exception>
    /// <returns>
    /// <see cref="CheckInResult"/> with the updated cycle day, streaks and saver status.
    /// </returns>
    Task<CheckInResult> CheckInAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
}