using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Common.Interfaces;
using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Entities;

public class Campaign : AuditableEntity, INormalizable
{
    public required Period Period { get; set; }
    public required string CampaignName { get; set; }
    public required string NormalizedCampaignName { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public required Location Location { get; set; }

    public void Normalize()
    {
        CampaignName = CampaignName.ToSanitized();
        NormalizedCampaignName = CampaignName.ToNormalized();
    }
}