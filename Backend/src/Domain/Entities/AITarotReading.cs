using MyTarotReader.Domain.Common;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Domain.Entities;

public class AITarotReading : BaseEntity
{
    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public CardCount CardCount { get; set; }

    public QuestionType QuestionType { get; set; }

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
