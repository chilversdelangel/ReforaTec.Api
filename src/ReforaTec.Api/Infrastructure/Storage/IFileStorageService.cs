namespace ReforaTec.Api.Infrastructure.Storage;

internal record FileStorageResult(string FileUrl, string FileIdentifier);

internal interface IFileStorageService
{
    public Task<FileStorageResult> UploadAsync(IFormFile file, string targetPath, CancellationToken cancellationToken = default);
}
