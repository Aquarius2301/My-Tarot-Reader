using MyTarotReader.Application.Dtos;

namespace MyTarotReader.Application.Contracts.Services;

/// <summary>Handles custom AI tarot chat sessions backed by Google Gemini.</summary>
public interface IAiChatService
{
    /// <summary>
    /// Creates a new chat session with the user's custom question and gets the AI's initial response.
    /// </summary>
    /// <param name="request">The chat session creation request.</param>
    /// <param name="userId">The authenticated user's id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The session id and the AI's initial response.</returns>
    /// <exception cref="Exceptions.BadRequestException">
    /// The question is empty or invalid.
    /// </exception>
    /// <exception cref="Exceptions.InternalServerException">
    /// Gemini call failed or returned no readable answer.
    /// </exception>
    Task<CreateChatSessionResponse> CreateChatSessionAsync(
        CreateChatSessionRequest request,
        Guid userId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Sends a message in an ongoing chat session and gets the AI's response.
    /// The server loads the conversation history from the database automatically.
    /// </summary>
    /// <param name="request">The chat message request.</param>
    /// <param name="userId">The authenticated user's id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The AI's response text.</returns>
    /// <exception cref="Exceptions.NotFoundException">
    /// The session was not found or does not belong to this user.
    /// </exception>
    /// <exception cref="Exceptions.BadRequestException">
    /// The session is not in chat phase, or conversation history is invalid.
    /// </exception>
    /// <exception cref="Exceptions.InternalServerException">
    /// Gemini call failed or returned no readable answer.
    /// </exception>
    Task<SendChatMessageResponse> SendChatMessageAsync(
        SendChatMessageRequest request,
        Guid userId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Submits drawn cards for reading in a custom chat session and gets the AI's interpretation.
    /// </summary>
    /// <param name="request">The custom reading request with session id and drawn cards.</param>
    /// <param name="userId">The authenticated user's id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The AI-generated interpretation text.</returns>
    /// <exception cref="Exceptions.NotFoundException">
    /// The session was not found or does not belong to this user.
    /// </exception>
    /// <exception cref="Exceptions.BadRequestException">
    /// The session is not in chat phase, card count is invalid, or card codes are invalid.
    /// </exception>
    /// <exception cref="Exceptions.InternalServerException">
    /// Gemini call failed or returned no readable answer.
    /// </exception>
    Task<CreateCustomReadingResponse> CreateCustomReadingAsync(
        CreateCustomReadingRequest request,
        Guid userId,
        CancellationToken cancellationToken = default
    );
}
