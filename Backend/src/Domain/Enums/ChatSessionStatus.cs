namespace MyTarotReader.Domain.Enums;

/// <summary>
/// Tracks the lifecycle of an AI chat session.
/// </summary>
public enum ChatSessionStatus
{
    /// <summary>The session is still in active conversation.</summary>
    Chatting,

    /// <summary>The session has been completed and persisted.</summary>
    Finished,
}
