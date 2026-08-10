namespace ReforaTec.Api.Entities.Common;

/// <summary>
/// Base class for entities that only require creation tracking, not modification tracking.
/// Use this instead of <see cref="AuditableEntity"/> for immutable entities like tokens or logs.
/// </summary>
public abstract class CreatableEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
}
