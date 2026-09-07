using MyTarotReader.Domain.Common;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Domain.Entities;

/// <summary>
/// A single message in an AI tarot chat conversation. Each record belongs to
/// an <see cref="AIChatHistory"/> and is ordered by its creation time. The entity inherits from <see cref="BaseEntity"/> to get the standard audit fields
/// (Id, CreatedAt, DeletedAt).
/// </summary>
public class ChatMessage : BaseEntity
{
    /// <summary>
    /// The identifier of the parent reading history this message belongs to.
    /// </summary>
    public Guid ChatId { get; set; }

    /// <summary>
    /// The role of the message sender: "user" or "model".
    /// </summary>
    public ChatRole Role { get; set; } = ChatRole.User;

    /// <summary>
    /// The text content of the message.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// JSON array string of the cards drawn for this reading, each entry containing
    /// the card's code, its English name, and whether it was reversed.
    /// Empty while still in chat phase.
    /// </summary>
    public string Cards { get; set; } = string.Empty;

    #region Navigation Properties

    /// <summary>
    /// Navigation property to the parent <see cref="AIChatHistory"/>.
    /// </summary>
    public AIChatHistory ChatHistory { get; set; } = null!;

    #endregion
}
