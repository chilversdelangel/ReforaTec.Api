using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Features.Campaigns.CreateCampaign;

public record Response(
    int Id,
    string CampaignName,
    string NormalizedCampaignName,
    Location Location,
    Period Period
);