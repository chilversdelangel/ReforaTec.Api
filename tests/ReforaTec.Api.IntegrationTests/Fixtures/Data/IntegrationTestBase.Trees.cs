using Microsoft.Extensions.DependencyInjection;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Common;
using ReforaTec.Api.Entities.Enums;

// ReSharper disable once CheckNamespace
namespace ReforaTec.Api.IntegrationTests.Fixtures;

public abstract partial class IntegrationTestBase
{
    private static int _treeFolioSequence = 100;

    protected async Task<Tree> CreateTreeAsync(
        int tenantId = 1,
        int speciesId = 1,
        int virtueId = 1,
        string? folio = null,
        
        DateOnly? plantingDate = null,
        decimal? heightCentimeters = 150.0m,
        decimal? diameterCentimeters = 15.0m,
        
        TreeHealthState healthState = TreeHealthState.Healthy,
        Location? location = null,
        string? notes = null)
    {
        using var scope = Factory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var tree = new Tree
        {
            TenantId = tenantId,
            Folio = folio ?? $"{Interlocked.Increment(ref _treeFolioSequence):D4}",
            SpeciesId = speciesId,
            VirtueId = virtueId,
            
            PlantingDate = plantingDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-6)),
            HeightCentimeters = heightCentimeters,
            DiameterCentimeters = diameterCentimeters,
            
            HealthState = healthState,
            Location = location,
            Notes = notes
        };

        tree.Normalize();

        appDbContext.Trees.Add(tree);
        await appDbContext.SaveChangesAsync();

        return tree;
    }
}
