namespace MyTarotReader.Domain.Enums;

/// <summary>
/// The horizon over which a crossroads decision is evaluated.
/// </summary>
public enum CrossroadsTimeFrame
{
    /// <summary> The decision has to be made right now. </summary>
    Now,

    /// <summary> The decision is expected within one to three months. </summary>
    OneToThreeMonths,

    /// <summary> The decision is expected further out, in more than six months. </summary>
    OverSixMonths,
}
