using MediatR;

namespace MyTarotReader.Application.Features.AiDeepTarot;

/// <summary>
/// Soft-deletes a deep tarot reading owned by the user.
/// </summary>
/// <param name="UserId">The authenticated user's ID.</param>
/// <param name="ReadingId">The ID of the reading to delete.</param>
public record DeleteAiDeepTarotReadingCommand(Guid UserId, Guid ReadingId) : IRequest<Unit>;
