namespace MyTarotReader.Domain.Enums;

/// <summary>
/// The type of a coin transaction.
/// </summary>
public enum TransactionType
{
    /// <summary>Coin spent on a one-shot AI tarot reading.</summary>
    AITarot,

    /// <summary>Coin spent on creating a new AI chat session (6 coins).</summary>
    AIChatSession,

    /// <summary>Coin spent on a follow-up chat phase after a reading (1 coin).</summary>
    AIChatFollowUp,

    /// <summary>White coin batch expired.</summary>
    Expired,
}
