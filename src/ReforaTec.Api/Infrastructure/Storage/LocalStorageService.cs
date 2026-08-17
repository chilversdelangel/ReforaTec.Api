namespace ReforaTec.Api.Infrastructure.Storage;

internal sealed class LocalStorageService(IWebHostEnvironment environment) : IFileStorageService
{
    public async Task<FileStorageResult> UploadAsync(IFormFile file, string folder, CancellationToken cancellationToken)
    {
        var fileName = file.FileName;
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var uniqueFileName = $"{Guid.NewGuid():N}{extension}";

        var webRoot = environment.WebRootPath;
        var normalizedFolder = folder.Trim('/', '\\');
        var targetDirectory = Path.Combine(webRoot, "media", normalizedFolder);

        Directory.CreateDirectory(targetDirectory);

        var physicalFilePath = Path.Combine(targetDirectory, uniqueFileName);
        await using var outputStream = new FileStream(physicalFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await file.CopyToAsync(outputStream, cancellationToken);

        var urlFolder = normalizedFolder.Replace('\\', '/');
        var fileKey = $"{urlFolder}/{uniqueFileName}";
        var url = $"/media/{fileKey}";

        return new FileStorageResult(url, fileKey);
    }
}
