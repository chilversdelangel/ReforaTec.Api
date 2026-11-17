namespace ReforaTec.Api.Features.Trees.CreateTreeCareLog;

internal sealed record Response(
    int Id,
    int TreeId,
    int CareTypeId,
    DateTime PerformedAt,
    DateTime DeviceCapturedAt,
    string? Notes,
    DateTime CreatedAt);
