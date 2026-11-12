using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Entities;

/// <summary>
/// Represents a student's enrollment in a specific campaign, acting as the waitlist
/// prior to tree assignment and holding the coordinator's final accreditation decision.
/// </summary>
public sealed class CampaignEnrollment : AuditableEntity
{
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    public int CampaignId { get; set; }
    public Campaign? Campaign { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    /// <summary>
    /// Final accreditation decision made by the coordinator at campaign closure.
    /// Null while the campaign is in progress or pending evaluation.
    /// </summary>
    public bool? IsApproved { get; set; }

    public string? EvaluationNotes { get; set; }
}
