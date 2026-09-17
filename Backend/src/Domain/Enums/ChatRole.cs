namespace MyTarotReader.Domain.Enums;

/// <summary> Represents a message that is sent by a user or a model (AI). </summary>
public enum ChatRole
{
    /// <summary> Represents a message is sent by a user. </summary>
    User,

    /// <summary>  Represents a message is sent by a model (AI). </summary>
    Model,
}
