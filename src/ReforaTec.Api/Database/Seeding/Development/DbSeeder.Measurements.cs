using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Seeding;

internal static partial class DbSeeder
{
    private static async Task SeedMeasurementsAsync(AppDbContext context)
    {
        if (await context.Measurements.AnyAsync())
        {
            return;
        }

        var user = await context.Users.FirstOrDefaultAsync(user => user.Email == "student@cdmadero.tecnm.mx");
        if (user is null)
        {
            return;
        }

        var tree = await context.Trees
            .Where(tree => tree.TenantId == user.TenantId)
            .OrderBy(tree => tree.Id)
            .FirstOrDefaultAsync();

        if (tree is null)
        {
            return;
        }

        var measurement1 = new Measurement
        {
            TenantId = user.TenantId,
            TreeId = tree.Id,
            InspectorId = user.Id,
            HeightCentimeters = 145m,
            DiameterCentimeters = 14.5m,
            PhotoUrl = "https://images.reforatec.org/evidence/tree1_baseline.jpg",
            DeviceCapturedAt = DateTime.UtcNow.AddMonths(-1)
        };

        var measurement2 = new Measurement
        {
            TenantId = user.TenantId,
            TreeId = tree.Id,
            InspectorId = user.Id,
            HeightCentimeters = 150.5m,
            DiameterCentimeters = 15.2m,
            PhotoUrl = "https://images.reforatec.org/evidence/tree1_latest.jpg",
            DeviceCapturedAt = DateTime.UtcNow.AddDays(-2)
        };

        context.Measurements.AddRange(measurement1, measurement2);
        await context.SaveChangesAsync();
    }
}
