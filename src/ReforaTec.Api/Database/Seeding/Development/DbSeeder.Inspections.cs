using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Database.Seeding;

internal static partial class DbSeeder
{
    private static async Task SeedInspectionsAsync(AppDbContext context)
    {
        if (await context.Inspections.AnyAsync())
        {
            return;
        }

        var user = await context.Users.FirstOrDefaultAsync(user => user.Email == "student@cdmadero.tecnm.mx");
        if (user is null)
        {
            return;
        }

        var trees = await context.Trees
            .Where(tree => tree.TenantId == user.TenantId)
            .OrderBy(tree => tree.Id)
            .Take(2)
            .ToListAsync();

        if (trees.Count < 2)
        {
            return;
        }

        var inspection1 = new Inspection
        {
            TenantId = user.TenantId,
            TreeId = trees[0].Id,
            UserId = user.Id,
            HealthState = TreeHealthState.Healthy,
            Notes = "Árbol muy pequeño pero verde.",
            DeviceCapturedAt = DateTime.UtcNow.AddDays(-2)
        };

        var inspection2 = new Inspection
        {
            TenantId = user.TenantId,
            TreeId = trees[1].Id,
            UserId = user.Id,
            HealthState = TreeHealthState.NeedsAttention,
            Notes = "Árbol recién registrado sin medidas iniciales.",
            DeviceCapturedAt = DateTime.UtcNow.AddDays(-1)
        };

        context.Inspections.AddRange(inspection1, inspection2);
        await context.SaveChangesAsync();
    }
}
