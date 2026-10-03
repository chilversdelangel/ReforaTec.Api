using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Storage;

namespace ReforaTec.Api.Features.Trees.UploadTreeMeasurementPhoto;

internal static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        int treeId,
        Request request,
        AppDbContext dbContext,
        IFileStorageService storageService,
        CancellationToken cancellationToken)
    {
        var treeExists = await dbContext.Trees
            .AnyAsync(t => t.Id == treeId, cancellationToken);

        if (!treeExists)
        {
            return Error.NotFound(ErrorCodes.TreeNotFound, $"Tree with id '{treeId}' was not found.");
        }

        var photoFile = request.TreePhoto;
        var targetPath = $"trees/{treeId}/measurements";

        var (fileUrl, fileIdentifier) = await storageService.UploadAsync(
            photoFile,
            targetPath,
            cancellationToken);

        return new Response(fileUrl, fileIdentifier, photoFile.Length);
    }
}
