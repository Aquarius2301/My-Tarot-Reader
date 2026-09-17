using MyTarotReader.Domain.Common;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Domain.Entities;

public class AIChatTarotReading : BaseEntity
{
    public Guid UserId { get; set; }

    public ChatSessionStatus Status { get; set; } = ChatSessionStatus.Chatting;

    /// <summary>
    /// Number of messages exchanged in the current chat phase
    /// (initial or follow-up), used to gate when a reading or payment is triggered.
    /// </summary>
    public int PhaseMessageCount { get; set; }

    /// <summary>
    /// Number of tarot readings performed so far within this chat session.
    /// </summary>
    public int ReadingCount { get; set; }

    /// <summary>
    /// Total coins spent across this entire chat session (initial + all follow-ups).
    /// </summary>
    public int TotalCoinSpent { get; set; }

    #region Navigation Properties
    public User User { get; set; } = null!;

    public ICollection<AIChatMessage> AIChatMessages { get; set; } = [];

    #endregion
}
