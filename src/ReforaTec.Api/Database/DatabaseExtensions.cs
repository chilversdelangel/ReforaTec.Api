using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ReforaTec.Api.Database;

internal static class DatabaseExtensions
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddPostgresDbContext(IConfiguration configuration)
        {
            var rawConnectionString = configuration.GetConnectionString("DefaultConnection");
            var connectionString = NormalizePostgresConnectionString(rawConnectionString);

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
            await dbContext.Database.MigrateAsync();
            await DbSeeder.SeedAsync(dbContext);
        }
    }

    private static string? NormalizePostgresConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) return connectionString;

        if (!connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString;
        }

        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':');
        var username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "";
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = port,
            Database = database,
            Username = username,
            Password = password,
            SslMode = SslMode.Prefer,
            TrustServerCertificate = true
        };

        return builder.ConnectionString;
    }
}