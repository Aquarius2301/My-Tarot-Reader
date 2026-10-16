using MyTarotReader.Domain.Common;

namespace MyTarotReader.Domain.Entities;

/// <summary>
/// Admin-managed spread configuration for an AI tarot reading, keyed by the number of cards.
/// </summary>
public class AITarotReadingCount : BaseEntity
{
    /// <summary> Number of cards drawn in this spread (e.g. 3, 5, 7, 10). </summary>
    public int CardCount { get; set; }

    /// <summary> White coin cost of a reading for this spread. </summary>
    public int WhiteCoins { get; set; }

    /// <summary> Position labels per card in drawn order, in English. </summary>
    public List<string> PositionsEn { get; set; } = [];

    /// <summary> Position labels per card in drawn order, in Vietnamese. </summary>
    public List<string> PositionsVi { get; set; } = [];

    /// <summary> Minimum response length (in words) requested from the AI. </summary>
    public int MinWords { get; set; }

    /// <summary> Maximum response length (in words) requested from the AI. </summary>
    public int MaxWords { get; set; }

    /// <summary> Whether this spread is offered to users. </summary>
    public bool IsEnabled { get; set; } = true;
}
