using MyTarotReader.Application.Dtos;

namespace MyTarotReader.Application.Contracts.Services;

/// <summary>Handles AI-powered tarot readings backed by Google Gemini.</summary>
public interface IAiTarotService
{
    /// <summary>
    /// Validates the reading request, asks Gemini to interpret the drawn cards
    /// against the question, persists an AIReadHistory record, and returns the answer.
    /// </summary>
    /// <param name="request">The reading request (cards, count, question).</param>
    /// <param name="userId">The authenticated user's id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The AI-generated answer text.</returns>
    /// <exception cref="Exceptions.BadRequestException">
    /// Invalid card code, mismatched card count, or missing custom question.
    /// </exception>
    /// <exception cref="Exceptions.InternalServerException">
    /// Gemini call failed or returned no readable answer.
    /// </exception>
    Task<CreateAiTarotReadingResponse> CreateAiTarotReadingAsync(
        CreateAiTarotReadingRequest request,
        Guid userId,
        CancellationToken cancellationToken = default
    );
}
