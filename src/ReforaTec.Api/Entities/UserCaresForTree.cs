using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Entities;

/// <summary>
/// Represents the temporal assignment of a user/student responsible for caring for a tree.
/// </summary>
public class UserCaresForTree : AuditableEntity
{
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    public int UserId { get; set; }
    public User? User { get; set; }
    
    public int TreeId { get; set; }
    public Tree? Tree { get; set; }

    /// <summary>
    /// Optional foreign key to Campaign. Null indicates the user is caring for the tree 
    /// independently outside a formal institutional campaign.
    /// </summary>
    public int? CampaignId { get; set; }
    public Campaign? Campaign { get; set; }
    
    public DateOnly StartDate { get; set; }
    
    /// <summary>
    /// End date of the care assignment. Null indicates the care relationship is currently active.
    /// </summary>
    public DateOnly? EndDate { get; set; }
}
