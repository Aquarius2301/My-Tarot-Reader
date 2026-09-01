namespace MyTarotReader.Domain.Common;

/// <summary>Base class for domain entities, providing a surrogate key and audit timestamps.</summary>
public class BaseEntity
{
    /// <summary>
    /// The id is a surrogate key that uniquely identifies the entity. It is generated when the entity is created and should not be modified afterward.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The creation timestamp is set when the entity is first persisted and should not be modified afterward.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// The deletion timestamp is set when the entity is marked for deletion and should not be modified afterward.
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; } = null;
}
