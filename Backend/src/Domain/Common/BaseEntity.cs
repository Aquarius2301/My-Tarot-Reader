namespace MyTarotReader.Domain.Common;

/// <summary>
/// Base entity class that provides common properties for all entities in the domain.
/// </summary>
public class BaseEntity
{
    /// <summary> Unique identifier for the entity. </summary>
    /// <remarks>
    /// The Id is already initialized by using <see cref="Guid.NewGuid()"/>.
    /// So don't need to set it manually when creating a new entity. It will be automatically generated.
    /// </remarks>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary> The date and time when the entity was created. </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary> The date and time when the entity was deleted. </summary>
    public DateTimeOffset? DeletedAt { get; set; } = null;
}
