namespace ReforaTec.Api.Features.Trees.CreateTreeCareLog;

internal sealed record Request(
    int CareTypeId,
    DateTimeOffset PerformedAt,
    DateTimeOffset DeviceCapturedAt,
    string? Notes);
