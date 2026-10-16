using MyTarotReader.Domain.Common;

namespace MyTarotReader.Domain.Entities;

/// <summary>
/// Admin-managed question type (topic) for an AI tarot reading.
/// </summary>
public class AiTarotReadingType : BaseEntity
{
    /// <summary> Stable code sent by clients and stored on readings (e.g. energy, love). </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary> Display name in English. </summary>
    public string NameEn { get; set; } = string.Empty;

    /// <summary> Display name in Vietnamese. </summary>
    public string NameVi { get; set; } = string.Empty;

    /// <summary> Whether this question type is offered to users. </summary>
    public bool IsEnabled { get; set; } = true;
}
