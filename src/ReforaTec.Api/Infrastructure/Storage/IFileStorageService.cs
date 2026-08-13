namespace ReforaTec.Api.Infrastructure.Storage;

internal interface IFileStorageService
{
    public Task<string> UploadAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);
}
