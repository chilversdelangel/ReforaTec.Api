using ErrorOr;
using ReforaTec.Api.Infrastructure.Storage;

namespace ReforaTec.Api.Features.Species.UploadSpeciesImage;

internal static class Handler
{
    private const string TargetPath = "catalog/species";

    public static async Task<ErrorOr<Response>> Handle(
        Request request,
        IFileStorageService storageService,
        CancellationToken cancellationToken = default)
    {
        var imageFile = request.SpeciesImage;
        var imageLength = imageFile.Length;

        var (fileUrl, fileIdentifier) = await storageService.UploadAsync(imageFile, TargetPath, cancellationToken);

        return new Response(fileUrl, fileIdentifier, imageLength);
    }
}
