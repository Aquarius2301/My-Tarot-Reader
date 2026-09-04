using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Application.Dtos;

/// <summary>A single tarot card drawn for an AI reading.</summary>
/// <param name="Code">The card's code (validated against <c>TarotConstants</c>).</param>
/// <param name="IsReversed">Whether the card was drawn reversed.</param>
public record AiTarotCardRequest(string Code, bool IsReversed);

/// <summary>
/// Request body for creating an AI tarot reading.
/// </summary>
/// <param name="CardCount">How many cards are drawn for the reading.</param>
/// <param name="QuestionType">The category of the user's question.</param>
/// <param name="Cards">The drawn cards, one entry per card.</param>
/// <param name="Language">The language for the AI response (<see cref="Language.Vi"/> or <see cref="Language.En"/>).</param>
public record CreateAiTarotReadingRequest(
    CardCount CardCount,
    QuestionType QuestionType,
    List<AiTarotCardRequest> Cards,
    Language Language
);

/// <summary>
/// Response returned after a successful AI tarot reading.
/// </summary>
/// <param name="Answer">The AI-generated interpretation text.</param>
public record CreateAiTarotReadingResponse(string Answer);

public enum Language
{
    /// <summary>Tiếng Việt (Vietnamese).</summary>
    Vi,

    /// <summary>English.</summary>
    En,
}

// ──────────────────────────────────────────────
//  Custom AI Chat DTOs
// ──────────────────────────────────────────────

/// <summary>A single message in the conversation history.</summary>
/// <param name="Role">The role: "user" or "model".</param>
/// <param name="Text">The message text content.</param>
public record ChatMessageDto(string Role, string Text);

/// <summary>
/// Request to start a new custom AI tarot chat session.
/// </summary>
/// <param name="Question">The user's custom free-text question.</param>
/// <param name="Language">The language for the AI response.</param>
public record CreateChatSessionRequest(string Question, Language Language);

/// <summary>
/// Response after creating a new chat session.
/// </summary>
/// <param name="HistoryId">The reading history identifier for follow-up messages.</param>
/// <param name="Answer">The AI's initial response to the user's question.</param>
public record CreateChatSessionResponse(Guid HistoryId, string Answer);

/// <summary>
/// Request to send a message in an ongoing chat session.
/// </summary>
/// <param name="HistoryId">The reading history to send the message to.</param>
/// <param name="Message">The user's new message text.</param>
/// <param name="Language">The language for the AI response.</param>
public record SendChatMessageRequest(
    Guid HistoryId,
    string Message,
    Language Language
);

/// <summary>
/// A spread position recommended by the AI.
/// </summary>
/// <param name="Position">The position number (1-based).</param>
/// <param name="Name">The meaning/name of this position.</param>
public record SpreadPositionDto(int Position, string Name);

/// <summary>
/// Structured spread recommendation parsed from the AI response.
/// Null when the AI has not yet recommended a spread.
/// </summary>
/// <param name="SpreadName">The name of the spread (e.g. "Past-Present-Future").</param>
/// <param name="CardCount">Number of cards to draw (1-15).</param>
/// <param name="Positions">The list of positions with their meanings.</param>
public record SpreadRecommendationDto(string SpreadName, int CardCount, List<SpreadPositionDto> Positions);

/// <summary>
/// Response after sending a chat message.
/// </summary>
/// <param name="Answer">The AI's response text.</param>
/// <param name="SpreadRecommendation">Parsed spread recommendation, null if AI hasn't proposed one yet.</param>
public record SendChatMessageResponse(string Answer, SpreadRecommendationDto? SpreadRecommendation);

/// <summary>
/// Request to submit cards for reading in a custom chat session.
/// </summary>
/// <param name="HistoryId">The reading history to associate this reading with.</param>
/// <param name="Cards">The drawn cards with codes and orientations. Card count = Cards.Count.</param>
/// <param name="Language">The language for the AI response.</param>
public record CreateCustomReadingRequest(
    Guid HistoryId,
    List<AiTarotCardRequest> Cards,
    Language Language
);

/// <summary>
/// Response after submitting cards for a custom reading.
/// </summary>
/// <param name="Answer">The AI-generated interpretation text.</param>
public record CreateCustomReadingResponse(string Answer);
