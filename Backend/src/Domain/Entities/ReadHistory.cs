using MyTarotReader.Domain.Common;

namespace MyTarotReader.Domain.Entities;

/// <summary>
/// Represents a record of a user drawing a tarot card. Each record stores the identifier of the
/// user who performed the draw and the code of the card that was drawn. The entity inherits from
/// <see cref="BaseEntity"/> to get the standard audit fields (Id, CreatedAt, DeletedAt).
/// </summary>
public class ReadHistory : BaseEntity
{
    /// <summary>
    /// The identifier of the user who drew the card.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The code of the tarot card that was drawn (e.g., "I", "II", "The Fool", etc.).
    /// </summary>
    public string CardCode { get; set; } = null!;

    /// <summary>
    /// Whether the drawn card was reversed (upside down).
    /// </summary>
    public bool IsReversed { get; set; }

    #region Navigation properties

    /// <summary>
    /// Navigation property to the <see cref="User"/> who performed the draw.
    /// </summary>
    public User User { get; set; } = null!;

    #endregion
}
