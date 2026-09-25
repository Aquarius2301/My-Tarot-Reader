using MyTarotReader.Domain.Common;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Domain.Entities;

public class AITarotReading : BaseEntity
{
    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public CardCount CardCount { get; set; }

    public QuestionType QuestionType { get; set; }

    /// <summary>
    /// A short excerpt of the answer (overview) used for list views,
    /// so callers do not have to fetch the full reading.
    /// </summary>
    public string AnswerSummary { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;

    /// <summary>
    /// The cards drawn for the tarot reading, represented as a string.
    /// </summary>
    /// <remarks>
    /// This property stores the cards by JSON serialization.
    /// </remarks>
    public string Cards { get; set; } = string.Empty;

    #region Navigation Properties
    public User User { get; set; } = null!;

    #endregion
}
