namespace ReforaTec.Api.Features.Trees.GetTreeCareLogs;

internal sealed record Response(IReadOnlyList<CareLogDto> CareLogs);

internal sealed record CareLogDto(
    int Id,
    int TreeId,
    int CareTypeId,
    string CareTypeName,
    int UserId,
    DateTime PerformedAt,
    DateTime DeviceCapturedAt,
    DateTime CreatedAt,
    string? Notes);
