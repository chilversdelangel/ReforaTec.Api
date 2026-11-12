using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Entities;

/// <summary>
/// Represents the temporal assignment of a student responsible for caring for a tree within a campaign.
/// </summary>
public sealed class CareAssignment : AuditableEntity
{
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    public int CampaignId { get; set; }
    public Campaign? Campaign { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int TreeId { get; set; }
    public Tree? Tree { get; set; }

    public DateOnly StartDate { get; set; }

    /// <summary>
    /// End date of the care assignment. Null indicates the care relationship is currently active.
    /// </summary>
    public DateOnly? EndDate { get; set; }
}
