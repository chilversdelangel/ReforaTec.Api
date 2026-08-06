using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;

namespace ReforaTec.Api.Infrastructure;

internal static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddPostgresDbContext(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
                options.UseSnakeCaseNamingConvention();
            });

            return services;
        }

        internal IServiceCollection AddCustomOpenApi()
        {
            services.AddOpenApi(options =>
            {
                options.CreateSchemaReferenceId = typeInfo => typeInfo.Type.FullName?
                    .Replace("ReforaTec.Api.Features.", "")
                    .Replace("+", ".");
            });

            return services;
        }
    }
}