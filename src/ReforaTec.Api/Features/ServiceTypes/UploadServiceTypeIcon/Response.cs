namespace ReforaTec.Api.Features.ServiceTypes.UploadServiceTypeIcon;

internal record Response(
    string FileUrl,
    string FileIdentifier,
    long SizeBytes
);
