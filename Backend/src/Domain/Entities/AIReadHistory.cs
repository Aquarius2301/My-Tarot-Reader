using MyTarotReader.Domain.Common;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Domain.Entities;

/// <summary>
/// Represents a record of a user performing an AI-powered tarot reading. Each record stores the
/// identifier of the user who performed the reading, the type of question, the number of cards
/// drawn, and the AI-generated answer. The entity inherits from
/// <see cref="BaseEntity"/> to get the standard audit fields (Id, CreatedAt, DeletedAt).
/// </summary>
public class AIReadHistory : BaseEntity
{
    /// <summary>
    /// The identifier of the user who performed the AI reading.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The number of tarot cards drawn for this reading (e.g., Three, Five, Seven, Ten).
    /// </summary>
    public CardCount CardCount { get; set; }

    /// <summary>
    /// The category of the user's question (e.g., Energy, Love, Career, Money).
    /// </summary>
    public QuestionType QuestionType { get; set; }

    /// <summary>
    /// The AI-generated answer or interpretation based on the drawn cards and the user's question.
    /// </summary>
    public string Answer { get; set; } = string.Empty;

    /// <summary>
    /// JSON array string of the cards drawn for this reading, each entry containing
    /// the card's code, its English name, and whether it was reversed
    /// (e.g. <c>[{"code":"maj-00","name":"The Fool","isReversed":true}]</c>).
    /// </summary>
    public string Cards { get; set; } = string.Empty;

    #region Navigation Properties

    /// <summary>
    /// Navigation property to the <see cref="User"/> who performed the AI reading.
    /// </summary>
    public User User { get; set; } = null!;

    #endregion
}
