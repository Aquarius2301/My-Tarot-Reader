using MyTarotReader.Domain.Common;

namespace MyTarotReader.Domain.Entities;

/// <summary>
/// A single message in an AI tarot chat conversation. Each record belongs to
/// an <see cref="AIReadHistory"/> and is ordered by <see cref="Sequence"/>.
/// </summary>
public class ChatMessage : BaseEntity
{
    /// <summary>
    /// The identifier of the parent reading history this message belongs to.
    /// </summary>
    public Guid HistoryId { get; set; }

    /// <summary>
    /// The role of the message sender: "user" or "model".
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// The text content of the message.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// The zero-based ordering index within the conversation.
    /// </summary>
    public int Sequence { get; set; }

    #region Navigation Properties

    /// <summary>
    /// Navigation property to the parent <see cref="AIReadHistory"/>.
    /// </summary>
    public AIReadHistory History { get; set; } = null!;

    #endregion
}
