using ReforaTec.Api.Features.Common.Dtos;

namespace ReforaTec.Api.Features.Campaigns.CreateCampaign;

internal sealed record Request(
    string Code,
    PeriodDto Period);
