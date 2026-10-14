namespace ReforaTec.Api.Features.Trees.CreateTreeService;

internal sealed record Request(
    int ServiceTypeId,
    DateTimeOffset DeviceCapturedAt,
    string? Comment
);
