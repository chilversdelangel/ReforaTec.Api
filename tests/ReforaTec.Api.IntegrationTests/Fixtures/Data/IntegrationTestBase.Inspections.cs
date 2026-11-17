using Microsoft.Extensions.DependencyInjection;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Enums;

// ReSharper disable once CheckNamespace
namespace ReforaTec.Api.IntegrationTests.Fixtures;

public abstract partial class IntegrationTestBase
{
    protected async Task<Inspection> RecordInspectionAsync(
        int treeId,
        int inspectorId,
        int tenantId = 1,
        TreeHealthState healthState = TreeHealthState.Healthy,
        string? notes = null,
        DateTime? deviceCapturedAt = null)
    {
        using var scope = Factory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var inspection = new Inspection
        {
            TenantId = tenantId,
            TreeId = treeId,
            InspectorId = inspectorId,
            HealthState = healthState,
            Notes = notes,
            DeviceCapturedAt = deviceCapturedAt ?? DateTime.UtcNow
        };

        appDbContext.Inspections.Add(inspection);
        await appDbContext.SaveChangesAsync();

        return inspection;
    }
}
