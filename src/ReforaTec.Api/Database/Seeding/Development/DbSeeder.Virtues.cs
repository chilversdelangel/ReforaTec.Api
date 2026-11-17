using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Seeding;

internal static partial class DbSeeder
{
    private static async Task SeedVirtuesAsync(AppDbContext context)
    {
        if (await context.Virtues.AnyAsync())
        {
            return;
        }

        var virtuesList = new List<Virtue>
        {
            new() { Name = "Responsabilidad" },
            new() { Name = "Respeto" },
            new() { Name = "Perseverancia" },
            new() { Name = "Cuidado Ambiental" },
            new() { Name = "Lealtad" }
        };

        foreach (var virtue in virtuesList)
        {
            virtue.Normalize();
            context.Virtues.Add(virtue);
        }

        await context.SaveChangesAsync();
    }
}
