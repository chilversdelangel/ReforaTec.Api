using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Features.Campaigns.CreateCampaign;

public record Request(
    string CampaignName,
    Location Location,
    Period Period
);