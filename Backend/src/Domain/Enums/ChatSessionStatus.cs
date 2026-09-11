namespace MyTarotReader.Domain.Enums;

/// <summary>
/// Tracks the lifecycle of an AI chat session.
/// </summary>
public enum ChatSessionStatus
{
    /// <summary>The session is still in active conversation (initial 5-message phase after paying 6 coins).</summary>
    Chatting,

    /// <summary>The session is waiting for the user to draw cards.</summary>
    DrawingCards,

    /// <summary>The last reading has finished; the user can pay 1 coin to continue chatting.</summary>
    ReadingDone,

    /// <summary>The session is in a follow-up conversation phase (3 messages after paying 1 coin).</summary>
    FollowUpChatting,

    /// <summary>The session has been completed and persisted.</summary>
    Finished,
}
