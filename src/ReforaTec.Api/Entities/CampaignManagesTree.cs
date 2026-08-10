using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Common.Interfaces;
using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Entities;

/// <summary>
/// Represents the enrollment and folio assignment of a tree within a specific campaign.
/// </summary>
public class CampaignManagesTree : AuditableEntity, INormalizable
{
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    public int CampaignId { get; set; }
    public Campaign? Campaign { get; set; }
    
    public int TreeId { get; set; }
    public Tree? Tree { get; set; }
    
    /// <summary>
    /// Human-readable folio identifier manually assigned by the campaign coordinator (e.g., "3" or "0034").
    /// </summary>
    public string CampaignFolio { get; set; } = string.Empty;
    public string NormalizedCampaignFolio { get; set; } = string.Empty;
    
    public DateOnly StartDate { get; set; }
    
    /// <summary>
    /// End date of the tree's participation in the campaign. Null indicates the tree is currently active in the campaign.
    /// </summary>
    public DateOnly? EndDate { get; set; }

    public void Normalize()
    {
        CampaignFolio = CampaignFolio.ToSanitized();
        NormalizedCampaignFolio = CampaignFolio.ToNormalized();
    }
}
