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
