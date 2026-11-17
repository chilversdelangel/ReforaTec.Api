using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Entities;

public sealed class CareLog : AuditableEntity
{
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int TreeId { get; set; }
    public Tree? Tree { get; set; }

    public int CareTypeId { get; set; }
    public CareType? CareType { get; set; }

    /// <summary>
    /// Timestamp when the care activity actually occurred in the field.
    /// </summary>
    public DateTime PerformedAt { get; set; }

    /// <summary>
    /// Timestamp captured on the mobile device when the care log was created offline.
    /// </summary>
    public DateTime DeviceCapturedAt { get; set; }

    public string? Notes { get; set; }
}
