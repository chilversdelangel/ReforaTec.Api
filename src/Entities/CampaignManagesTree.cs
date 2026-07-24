using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Common.Interfaces;
using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Entities;

public class CampaignManagesTree : AuditableEntity, INormalizable
{
    public int CampaignId { get; set; }
    public Campaign? Campaign { get; set; }
    
    public int TreeId { get; set; }
    public Tree? Tree { get; set; }
    
    public string CampaignFolio { get; set; } = string.Empty;
    public string NormalizedCampaignFolio { get; set; } = string.Empty;
    
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    public void Normalize()
    {
        CampaignFolio = CampaignFolio.ToSanitized();
        NormalizedCampaignFolio = CampaignFolio.ToNormalized();
    }
}
