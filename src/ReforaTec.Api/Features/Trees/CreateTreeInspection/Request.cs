namespace ReforaTec.Api.Features.Trees.CreateTreeInspection;

internal sealed record Request(
    string HealthState,
    string? Notes,
    DateTimeOffset DeviceCapturedAt);
