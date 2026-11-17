using ReforaTec.Api.Entities.Common;
using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Entities;

public sealed class Inspection : AuditableEntity
{
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    public int TreeId { get; set; }
    public Tree? Tree { get; set; }

    public int InspectorId { get; set; }
    public User? Inspector { get; set; }

    public TreeHealthState HealthState { get; set; } = TreeHealthState.Healthy;
    public string? Notes { get; set; }

    public DateTime DeviceCapturedAt { get; set; }
}
