using MyTarotReader.Domain.Common;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Domain.Entities;

/// <summary>
/// Represents a record of a user performing an AI-powered tarot reading. Each record stores the
/// identifier of the user who performed the reading, the type of question, the number of cards
/// drawn, the AI-generated answer, and the conversation history as separate <see cref="ChatMessage"/>
/// records. The entity inherits from <see cref="BaseEntity"/> to get the standard audit fields
/// (Id, CreatedAt, DeletedAt).
/// </summary>
public class AIReadHistory : BaseEntity
{
    /// <summary>
    /// The identifier of the user who performed the AI reading.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The current lifecycle status of this reading (Chat or Reading).
    /// </summary>
    public ChatSessionStatus Status { get; set; } = ChatSessionStatus.Chat;

    /// <summary>
    /// The number of tarot cards drawn for this reading (e.g., Three, Five, Seven, Ten).
    /// Null while still in chat phase or for custom readings where the count is determined by the Cards array length.
    /// </summary>
    public CardCount? CardCount { get; set; }

    /// <summary>
    /// The category of the user's question (e.g., Energy, Love, Career, Money, Custom).
    /// Null while still in chat phase.
    /// </summary>
    public QuestionType? QuestionType { get; set; }

    /// <summary>
    /// The user's free-text question for this reading.
    /// </summary>
    public string Question { get; set; } = string.Empty;

    /// <summary>
    /// The AI-generated answer or interpretation based on the drawn cards and the user's question.
    /// Null while still in chat phase.
    /// </summary>
    public string? Answer { get; set; }

    /// <summary>
    /// JSON array string of the cards drawn for this reading, each entry containing
    /// the card's code, its English name, and whether it was reversed.
    /// Null while still in chat phase.
    /// </summary>
    public string? Cards { get; set; }

    #region Navigation Properties

    /// <summary>
    /// Navigation property to the <see cref="User"/> who performed the AI reading.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Navigation property to the conversation messages for this reading.
    /// </summary>
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

    #endregion
}
