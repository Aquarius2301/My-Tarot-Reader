using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Application.Dtos;

/// <summary>
/// Represents a single AI read-history record returned to the client.
/// </summary>
/// <param name="Id">The unique identifier of the AI read-history record.</param>
/// <param name="CardCount">The number of tarot cards drawn for this reading.</param>
/// <param name="QuestionType">The category of the user's question.</param>
/// <param name="Answer">The AI-generated interpretation text.</param>
/// <param name="Cards">JSON array string of the cards drawn.</param>
/// <param name="CreatedAt">The UTC timestamp when the reading was created.</param>
public record AIReadHistoryResult(
    Guid Id,
    CardCount CardCount,
    QuestionType QuestionType,
    string Answer,
    string Cards,
    DateTimeOffset CreatedAt
);
