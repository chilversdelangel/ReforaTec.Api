using Microsoft.Extensions.DependencyInjection;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;

// ReSharper disable once CheckNamespace
namespace ReforaTec.Api.IntegrationTests.Fixtures;

public abstract partial class IntegrationTestBase
{
    protected async Task<Measurement> RecordMeasurementAsync(
        int treeId,
        int inspectorId,
        int tenantId = 1,
        decimal heightCentimeters = 150.0m,
        decimal diameterCentimeters = 15.0m,
        string? photoUrl = null,
        DateTime? deviceCapturedAt = null)
    {
        using var scope = Factory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var measurement = new Measurement
        {
            TenantId = tenantId,
            TreeId = treeId,
            InspectorId = inspectorId,
            HeightCentimeters = heightCentimeters,
            DiameterCentimeters = diameterCentimeters,
            PhotoUrl = photoUrl,
            DeviceCapturedAt = deviceCapturedAt ?? DateTime.UtcNow
        };

        appDbContext.Measurements.Add(measurement);
        await appDbContext.SaveChangesAsync();

        return measurement;
    }
}
