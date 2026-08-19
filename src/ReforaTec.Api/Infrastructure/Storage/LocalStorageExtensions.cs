using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace ReforaTec.Api.Infrastructure.Storage;

internal static class LocalStorageExtensions
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddLocalStorage(IConfiguration configuration)
        {
            var localStorageSection = configuration.GetSection(LocalStorageOptions.SectionName);

            services.Configure<LocalStorageOptions>(localStorageSection);
            services.AddScoped<IFileStorageService, LocalStorageService>();

            return services;
        }
    }

    extension(WebApplication app)
    {
        internal WebApplication UseLocalStorage()
        {
            var options = app.Services.GetRequiredService<IOptions<LocalStorageOptions>>().Value;
            var environment = app.Environment;

            var storageRoot = Path.GetFullPath(options.StoragePath, environment.ContentRootPath);
            Directory.CreateDirectory(storageRoot);

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(storageRoot),
                RequestPath = "/media"
            });

            return app;
        }
    }
}
