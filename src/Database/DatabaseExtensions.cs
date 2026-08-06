using Microsoft.EntityFrameworkCore;

namespace ReforaTec.Api.Database;

internal static class DatabaseExtensions
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
    }

    extension(WebApplication app)
    {
        internal async Task SeedDatabaseAsync()
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbSeeder.SeedAsync(dbContext);
        }
    }
}