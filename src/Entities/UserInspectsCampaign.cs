using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Entities;

/// <summary>
/// Represents the temporal assignment of an inspector to a campaign.
/// </summary>
public class UserInspectsCampaign : AuditableEntity
{
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    public int UserId { get; set; }
    public User? User { get; set; }
    
    public int CampaignId { get; set; }
    public Campaign? Campaign { get; set; }
    
    public DateOnly StartDate { get; set; }
    
    /// <summary>
    /// End date of the assignment. Null indicates the inspector is currently active in the campaign.
    /// </summary>
    public DateOnly? EndDate { get; set; }
}
