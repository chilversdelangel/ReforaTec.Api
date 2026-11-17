using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Entities;

public sealed class Measurement : AuditableEntity
{
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    public int TreeId { get; set; }
    public Tree? Tree { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public decimal HeightCentimeters { get; set; }
    public decimal DiameterCentimeters { get; set; }

    public string? PhotoUrl { get; set; }

    public DateTime DeviceCapturedAt { get; set; }
}
