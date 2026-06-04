using ReforaTec.Api.Features.Common.Dtos;

namespace ReforaTec.Api.Features.Campaigns.GetCampaignById;

public record Response(
    int Id,
    string CampaignName,
    string NormalizedCampaignName,
    PeriodDto Period,
    string SchoolName,
    LocationDto Location
);