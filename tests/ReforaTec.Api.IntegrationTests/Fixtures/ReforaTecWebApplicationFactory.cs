using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using Respawn;
using Testcontainers.PostgreSql;

namespace ReforaTec.Api.IntegrationTests.Fixtures;

public class ReforaTecWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:18")
        .WithDatabase("reforatec_test")
        .WithUsername("postgres")
        .WithPassword("admin")
        .Build();

    private Respawner? _respawner;
    private NpgsqlConnection? _dbConnection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        const string connectionStringKey = "ConnectionStrings:DefaultConnection";
        var connectionString = _dbContainer.GetConnectionString();

        builder.UseSetting(connectionStringKey, connectionString);
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // Forces the in-memory server to start to run migrations and seeders
        _ = Server;

        await SeedForeignTenantAsync();

        var connectionString = _dbContainer.GetConnectionString();
        _dbConnection = new NpgsqlConnection(connectionString);

        await _dbConnection.OpenAsync();

        _respawner = await CreateRespawnerAsync(_dbConnection);
    }

    private async Task SeedForeignTenantAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (await context.Tenants.CountAsync() >= 2)
        {
            return;
        }

        var foreignTenant = new Tenant
        {
            InstitutionName = "Instituto Tecnológico de Tampico",
            Acronym = "ITT",
            InstitutionalEmailDomain = "tampico.tecnm.mx"
        };
        foreignTenant.Normalize();

        context.Tenants.Add(foreignTenant);
        await context.SaveChangesAsync();
    }

    public Task ResetDatabaseAsync()
    {
        if (_respawner is null || _dbConnection is null)
        {
            throw new InvalidOperationException(
                "Cannot reset database: ReforaTecWebApplicationFactory has not been initialized.");
        }

        return _respawner.ResetAsync(_dbConnection);
    }

    public new async Task DisposeAsync()
    {
        if (_dbConnection is not null)
        {
            await _dbConnection.DisposeAsync();
        }

        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    private static Task<Respawner> CreateRespawnerAsync(NpgsqlConnection dbConnection)
    {
        return Respawner.CreateAsync(dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore =
            [
                "__EFMigrationsHistory",
                "tenants",
                "species",
                "values",
                "service_types",
                "notification_templates"
            ]
        });
    }
}