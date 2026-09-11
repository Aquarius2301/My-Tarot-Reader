using MyTarotReader.Domain.Common;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Domain.Entities;

/// <summary>
/// Represents an AI tarot chat session. Each session belongs to a single user and holds a
/// conversation as a collection of <see cref="ChatMessage"/> records, together with its current
/// lifecycle <see cref="Status"/>. Inherits from <see cref="BaseEntity"/> for the standard audit
/// fields (Id, CreatedAt, DeletedAt).
/// </summary>
public class AIChatHistory : BaseEntity
{
    /// <summary>
    /// The identifier of the user who performed the AI reading.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The current lifecycle status of this reading (Chat or Reading).
    /// </summary>
    public ChatSessionStatus Status { get; set; } = ChatSessionStatus.Chatting;

    /// <summary>
    /// The number of user messages sent in the current phase. Resets to zero whenever the
    /// session transitions to a new phase (e.g. after a reading). Bounded by the phase
    /// message limit (5 in the initial <see cref="ChatSessionStatus.Chatting"/> phase and
    /// 3 in each <see cref="ChatSessionStatus.FollowUpChatting"/> phase).
    /// </summary>
    public int PhaseMessageCount { get; set; }

    /// <summary>
    /// The total number of readings performed in this session. Zero before the first reading;
    /// any value &gt; 0 means the session is in a follow-up cycle and only a single card may be drawn.
    /// </summary>
    public int ReadingCount { get; set; }

    /// <summary>
    /// The total number of coins spent on this session (6 at creation plus 1 per follow-up phase).
    /// </summary>
    public int TotalCoinSpent { get; set; }

    #region Navigation Properties

    /// <summary>
    /// Navigation property to the <see cref="User"/> who performed the AI reading.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Navigation property to the collection of <see cref="ChatMessage"/>s that belong to
    /// this reading. The messages are ordered by <see cref="ChatMessage"/>.
    /// </summary>
    public ICollection<ChatMessage> Messages { get; set; } = [];

    #endregion
}
