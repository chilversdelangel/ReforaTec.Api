using Microsoft.Extensions.Options;
using ReforaTec.Api.Common.Helpers;

namespace ReforaTec.Api.Infrastructure.Storage;

internal sealed class LocalStorageService(
    IWebHostEnvironment environment,
    IOptions<LocalStorageOptions> options) : IFileStorageService
{
    private readonly LocalStorageOptions _options = options.Value;
    private readonly string _storageRoot = Path.GetFullPath(options.Value.StoragePath, environment.ContentRootPath);

    public async Task<FileStorageResult> UploadAsync(
        IFormFile file,
        string targetPath,
        CancellationToken cancellationToken)
    {
        var fileName = file.FileName;
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var uniqueFileName = $"{Guid.NewGuid():N}{extension}";

        targetPath = targetPath.ToNormalizedPath();

        var directory = Path.Combine(_storageRoot, targetPath);
        Directory.CreateDirectory(directory);

        var filePath = Path.Combine(directory, uniqueFileName);
        await using var fileStream = File.Create(filePath);
        await file.CopyToAsync(fileStream, cancellationToken);

        var fileIdentifier = $"{targetPath}/{uniqueFileName}";
        var baseUrl = _options.BaseUrl.TrimEnd('/');
        var fileUrl = $"{baseUrl}/media/{fileIdentifier}";

        return new FileStorageResult(fileUrl, fileIdentifier);
    }
}
