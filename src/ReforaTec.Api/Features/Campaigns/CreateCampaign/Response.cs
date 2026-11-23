using ReforaTec.Api.Features.Common.Dtos;

namespace ReforaTec.Api.Features.Campaigns.CreateCampaign;

internal sealed record Response(
    int Id,
    string Code,
    PeriodDto Period);
