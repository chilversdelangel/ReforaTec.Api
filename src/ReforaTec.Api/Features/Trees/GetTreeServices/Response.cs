namespace ReforaTec.Api.Features.Trees.GetTreeServices;

internal sealed record Response(
    int Id,
    int TreeId,
    int ServiceTypeId,
    string ServiceTypeName,
    int StudentId,
    int? CampaignId,
    DateTime DeviceCapturedAt,
    DateTime CreatedAt,
    string? Comment);
