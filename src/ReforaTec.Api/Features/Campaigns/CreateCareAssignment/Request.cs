namespace ReforaTec.Api.Features.Campaigns.CreateCareAssignment;

internal sealed record Request(
    int UserId,
    int TreeId,
    DateOnly StartDate);
