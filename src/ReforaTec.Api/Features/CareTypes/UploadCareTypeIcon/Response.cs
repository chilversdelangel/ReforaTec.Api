namespace ReforaTec.Api.Features.CareTypes.UploadCareTypeIcon;

internal sealed record Response(
    string FileUrl,
    string FileIdentifier,
    long SizeBytes);
