using ErrorOr;
using ReforaTec.Api.Infrastructure.Storage;

namespace ReforaTec.Api.Features.ServiceTypes.UploadServiceTypeIcon;

internal static class Handler
{
    private const string TargetPath = "catalog/service-types";

    public static async Task<ErrorOr<Response>> Handle(
        Request request,
        IFileStorageService storageService,
        CancellationToken cancellationToken = default)
    {
        var iconFile = request.IconFile;
        var iconLength = iconFile.Length;

        var (fileUrl, fileIdentifier) = await storageService.UploadAsync(iconFile, TargetPath, cancellationToken);

        return new Response(fileUrl, fileIdentifier, iconLength);
    }
}
