using MyTarotReader.Domain.Common;

namespace MyTarotReader.Domain.Entities;

public class TarotReading : BaseEntity
{
    public Guid UserId { get; set; }

    /// <summary>
    /// The identifying code of the drawn tarot card (e.g. "maj-00", "min-wands-1").
    /// </summary>
    public string CardCode { get; set; } = null!;

    /// <summary>
    /// Whether the card was drawn in reversed orientation.
    /// </summary>
    public bool IsReversed { get; set; }

    #region Navigation properties

    public User User { get; set; } = null!;

    #endregion
}
