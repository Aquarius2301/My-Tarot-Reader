namespace MyTarotReader.Domain.Enums;

/// <summary> Represents the role of a user in the system. </summary>
public enum UserRole
{
    /// <summary> Represents a user who has registered but has not yet upgraded to a Pro account. </summary>
    Registered,

    /// <summary> Represents a user who has upgraded to a Pro account and has access to additional features. </summary>
    Pro,
}
