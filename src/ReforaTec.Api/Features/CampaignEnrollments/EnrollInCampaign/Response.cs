using ReforaTec.Api.Features.Common.Dtos;

namespace ReforaTec.Api.Features.CampaignEnrollments.EnrollInCampaign;

internal sealed record Response(
    int Id,
    int CampaignId,
    string CampaignCode,
    PeriodDto Period);
