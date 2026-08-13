namespace ReforaTec.Api.Infrastructure.Storage;

internal static class StorageExtensions
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddFileStorage()
        {
            services.AddScoped<IFileStorageService, LocalStorageService>();
            
            return services;
        }
    }
}
