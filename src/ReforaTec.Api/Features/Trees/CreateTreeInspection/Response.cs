namespace ReforaTec.Api.Features.Trees.CreateTreeInspection;

internal sealed record Response(
    int Id,
    int TreeId,
    string HealthState,
    string? Notes,
    DateTime DeviceCapturedAt,
    DateTime CreatedAt);
