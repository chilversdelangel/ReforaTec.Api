using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Features.Trees.CreateTreeInspection;

internal sealed record Response(
    int Id,
    int TreeId,
    TreeHealthState HealthState,
    string? Notes,
    DateTime DeviceCapturedAt,
    DateTime CreatedAt);
