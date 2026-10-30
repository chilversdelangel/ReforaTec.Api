using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Database.Seeding;

internal static partial class DbSeeder
{
    private static async Task SeedTreesAsync(AppDbContext context)
    {
        if (await context.Trees.AnyAsync())
        {
            return;
        }

        var tenant = await context.Tenants.FirstAsync(tenant => tenant.Acronym == "ITCM");
        var speciesList = await context.Species.Take(2).ToListAsync();
        var value = await context.Values.FirstAsync();

        if (speciesList.Count < 2)
        {
            return;
        }

        var tree1 = new Tree
        {
            TenantId = tenant.Id,
            SpeciesId = speciesList[0].Id,
            ValueId = value.Id,
            HealthState = TreeHealthState.Healthy,
            PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
            HeightCentimeters = 150.5m,
            DiameterCentimeters = 15.2m,
            Notes = "Árbol veterano plantado cerca del edificio de sistemas."
        };

        var tree2 = new Tree
        {
            TenantId = tenant.Id,
            SpeciesId = speciesList[1].Id,
            ValueId = value.Id,
            HealthState = TreeHealthState.NeedsAttention,
            PlantingDate = null,
            HeightCentimeters = null,
            DiameterCentimeters = null,
            Notes = "Árbol recién registrado sin medidas iniciales."
        };

        context.Trees.AddRange(tree1, tree2);
        await context.SaveChangesAsync();
    }
}
