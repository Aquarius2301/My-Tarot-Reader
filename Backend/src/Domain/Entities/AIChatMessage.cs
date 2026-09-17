using MyTarotReader.Domain.Common;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Domain.Entities;

public class AIChatMessage : BaseEntity
{
    public Guid ChatId { get; set; }

    public ChatRole Role { get; set; } = ChatRole.User;

    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// The cards drawn for the tarot reading, represented as a string.
    /// </summary>
    /// <remarks>
    /// This property stores the cards by JSON serialization.
    /// </remarks>
    public string Cards { get; set; } = string.Empty;

    #region Navigation Properties
    public AIChatTarotReading AIChatTarotReading { get; set; } = null!;

    #endregion
}
