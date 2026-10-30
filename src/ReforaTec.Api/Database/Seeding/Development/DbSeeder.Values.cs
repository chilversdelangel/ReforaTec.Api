using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Seeding;

internal static partial class DbSeeder
{
    private static async Task SeedValuesAsync(AppDbContext context)
    {
        if (await context.Values.AnyAsync())
        {
            return;
        }

        var valuesList = new List<Value>
        {
            new() { ValueName = "Responsabilidad" },
            new() { ValueName = "Respeto" },
            new() { ValueName = "Perseverancia" },
            new() { ValueName = "Cuidado Ambiental" },
            new() { ValueName = "Lealtad" }
        };

        foreach (var value in valuesList)
        {
            value.Normalize();
            context.Values.Add(value);
        }

        await context.SaveChangesAsync();
    }
}
