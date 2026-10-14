using MediatR;

namespace MyTarotReader.Application.Features.Streak;

/// <summary>
/// Retrieves the current streak information for the authenticated user.
/// </summary>
/// <param name="UserId">The authenticated user's ID.</param>
/// <remarks>
/// The streak saver is automatically reset at the start of a new month (Vietnam time),
/// and a broken streak is reset on read.
/// </remarks>
public record GetStreakQuery(Guid UserId) : IRequest<GetStreakResult>;

/// <summary>
/// Result of retrieving the current streak information for a user.
/// </summary>
/// <param name="CycleDay">The current day in the user's cycle (0 to CycleDays - 1).</param>
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
