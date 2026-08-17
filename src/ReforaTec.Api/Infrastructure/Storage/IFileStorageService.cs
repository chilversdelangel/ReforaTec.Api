namespace ReforaTec.Api.Infrastructure.Storage;

internal record FileStorageResult(string Url, string FileKey);

internal interface IFileStorageService
{
    public Task<FileStorageResult> UploadAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);
}
