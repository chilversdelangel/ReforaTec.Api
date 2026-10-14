namespace ReforaTec.Api.Features.Trees.CreateTreeService;

internal sealed record Response(
    int Id,
    int TreeId,
    int ServiceTypeId,
    int? CampaignId,
    DateTime DeviceCapturedAt,
    string? Comment,
    DateTime CreatedAt
);
