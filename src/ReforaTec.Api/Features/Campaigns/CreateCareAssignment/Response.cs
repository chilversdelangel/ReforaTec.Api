namespace ReforaTec.Api.Features.Campaigns.CreateCareAssignment;

internal sealed record Response(
    int Id,
    int CampaignId,
    int UserId,
    int TreeId,
    DateOnly StartDate,
    DateOnly? EndDate);
