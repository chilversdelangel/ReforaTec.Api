using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Features.Trees.CreateTreeInspection;

internal sealed record Request(
    TreeHealthState HealthState,
    string? Notes,
    DateTimeOffset DeviceCapturedAt);
