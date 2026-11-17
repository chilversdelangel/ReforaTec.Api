using Microsoft.Extensions.DependencyInjection;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;

// ReSharper disable once CheckNamespace
namespace ReforaTec.Api.IntegrationTests.Fixtures;

public abstract partial class IntegrationTestBase
{
    protected async Task<CareLog> CreateCareLogAsync(
        int treeId,
        int userId,
        int careTypeId = 1,
        int tenantId = 1,
        DateTime? performedAt = null,
        DateTime? deviceCapturedAt = null,
        string? notes = null)
    {
        using var scope = Factory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTime.UtcNow;

        var careLog = new CareLog
        {
            TenantId = tenantId,
            TreeId = treeId,
            UserId = userId,
            CareTypeId = careTypeId,
            PerformedAt = performedAt ?? now,
            DeviceCapturedAt = deviceCapturedAt ?? now,
            Notes = notes
        };

        appDbContext.CareLogs.Add(careLog);
        await appDbContext.SaveChangesAsync();

        return careLog;
    }
}
