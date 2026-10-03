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
        internal async Task MigrateDatabaseAsync()
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.MigrateAsync();
        }

        internal async Task SeedDatabaseAsync()
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbSeeder.SeedAsync(dbContext);
        }
    }

    private static string? NormalizePostgresConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        if (!connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString;
        }

        var uri = new Uri(connectionString);
        var (username, password) = uri.UserInfo.Split(':') switch
        {
            [var user, var pass] => (Uri.UnescapeDataString(user), Uri.UnescapeDataString(pass)),
            [var user] => (Uri.UnescapeDataString(user), string.Empty),
            _ => (string.Empty, string.Empty)
        };
        const int defaultPostgresPort = 5432;
        var port = uri.Port > 0 ? uri.Port : defaultPostgresPort;
        var databaseName = uri.AbsolutePath.TrimStart('/');

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = port,
            Database = databaseName,
            Username = username,
            Password = password,
            SslMode = SslMode.Prefer
        };

        return builder.ConnectionString;
    }
}