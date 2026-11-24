using ReforaTec.Api.Features.Common.Dtos;

namespace ReforaTec.Api.Features.Campaigns.GetCampaignById;

internal sealed record Response(
    int Id,
    string Code,
    PeriodDto Period);
