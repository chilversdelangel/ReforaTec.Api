namespace ReforaTec.Api.Features.CareAssignments.CloseCareAssignment;

internal sealed record Response(
    int Id,
    int CampaignId,
    int UserId,
    int TreeId,
    DateOnly StartDate,
    DateOnly EndDate);
