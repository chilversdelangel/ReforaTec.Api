namespace ReforaTec.Api.Features.Trees.CreateTreeMeasurement;

internal sealed record Request(
    decimal HeightCentimeters,
    DateTimeOffset DeviceCapturedAt);
