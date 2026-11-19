namespace ReforaTec.Api.Features.Trees.CreateTreeMeasurement;

internal sealed record Response(
    int Id,
    int TreeId,
    decimal HeightCentimeters,
    DateTime DeviceCapturedAt,
    DateTime CreatedAt);
