using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Seeding;

internal static partial class DbSeeder
{
    private static async Task SeedTreesAsync(AppDbContext context)
    {
        if (await context.Trees.AnyAsync())
        {
            return;
        }

        var tenant = await context.Tenants
            .FirstAsync(tenant => tenant.Acronym == "ITCM");
        
        var speciesList = await context.Species
            .OrderBy(species => species.Id)
            .Take(2)
            .ToListAsync();
        
        var virtue = await context.Virtues
            .OrderBy(virtue => virtue.Id)
            .FirstAsync();

        if (speciesList.Count < 2)
        {
            return;
        }

        var tree1 = new Tree
        {
            TenantId = tenant.Id,
            Folio = "01",
            SpeciesId = speciesList[0].Id,
            VirtueId = virtue.Id,
            PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        };
        tree1.Normalize();

        var tree2 = new Tree
        {
            TenantId = tenant.Id,
            Folio = "02",
            SpeciesId = speciesList[1].Id,
            VirtueId = virtue.Id,
            PlantingDate = null
        };
        tree2.Normalize();

        context.Trees.AddRange(tree1, tree2);
        await context.SaveChangesAsync();
    }
}
