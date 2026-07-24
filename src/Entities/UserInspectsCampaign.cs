using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Entities;

public class UserInspectsCampaign : AuditableEntity
{
    public int UserId { get; set; }
    public User? User { get; set; }
    
    public int CampaignId { get; set; }
    public Campaign? Campaign { get; set; }
    
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}
