using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Common.Interfaces;
using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Entities;

/// <summary>
/// Represents an institutional environmental campaign within an educational institution.
/// </summary>
public class Campaign : AuditableEntity, INormalizable
{
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    public required Period Period { get; set; }
    
    public string CampaignName { get; set; } = string.Empty;
    public string NormalizedCampaignName { get; set; } = string.Empty;
    
    /// <summary>
    /// Auto-generated enrollment code used by students to join the campaign (e.g. "ITCM-2026-A").
    /// </summary>
    public string InscriptionCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional geographic GPS boundary or campus zone location. Null if drafting 
    /// the campaign from the web dashboard before specifying exact campus coordinates.
    /// </summary>
    public Location? Location { get; set; }

    public void Normalize()
    {
        CampaignName = CampaignName.ToSanitized();
        NormalizedCampaignName = CampaignName.ToNormalized();
        
        InscriptionCode = InscriptionCode.ToSanitized().ToUpperInvariant();
    }
}