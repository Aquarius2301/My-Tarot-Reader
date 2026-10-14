using MediatR;

namespace MyTarotReader.Application.Features.Streak;

/// <summary>
/// Performs a daily check-in for the authenticated user, creating a new streak when
/// none exists and granting the cycle reward as white coins.
/// </summary>
/// <param name="UserId">The authenticated user's ID.</param>
public record CheckInCommand(Guid UserId) : IRequest<Unit>;
