namespace MyTarotReader.Domain.Enums;

/// <summary> The type of a coin transaction. </summary>
public enum OrderType
{
    ///  ------------ Deduct ---------------
    /// <summary> Deduct coins for AI tarot reading</summary>
    AITarot,

    /// <summary> Deduct coins for create a new AI chat session </summary>
    CreateChatSession,

    /// <summary> Deduct coins for AI chat follow-up conversation. </summary>
    AIChatFollowUp,

    /// <summary> Deduct coins for expired coins. </summary>
    Expired,

    ///  ------------ Top Up ---------------
    /// <summary> Top up coins for user.  </summary>
    TopUp,

    /// <summary> Grant white coins for the daily check-in. </summary>
    DailyCheckIn,

    /// <summary> Grant the one-time white coins for the first login (sign-up bonus). </summary>
    FirstLogin,

    /// <summary> Swap red coins into white coins (1 red = 2 white). </summary>
    RedToWhite,
}
