using ReforaTec.Api.Features.Common.Dtos;

namespace ReforaTec.Api.Features.Campaigns.CloseCampaign;

internal sealed record Response(
    int CampaignId,
    string CampaignCode,
    PeriodDto Period,
    int ClosedAssignmentsCount);
