namespace ReforaTec.Api.Infrastructure.Storage;

internal sealed class LocalStorageOptions
{
    public const string SectionName = "LocalStorage";

    public string BaseUrl { get; set; } = string.Empty;
    public string StoragePath { get; set; } = "storage";
}
