using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Entities;

public class UserCaresForTree : AuditableEntity
{
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
    public DateOnly? EndDate { get; set; }
}
