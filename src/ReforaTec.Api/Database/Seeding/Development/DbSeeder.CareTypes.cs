using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Seeding;

internal static partial class DbSeeder
{
    private static async Task SeedCareTypesAsync(AppDbContext context)
    {
        if (await context.CareTypes.AnyAsync())
        {
            return;
        }

        var careTypesList = new List<CareType>
        {
            new()
            {
                Name = "Riego",
                IconUrl = "https://icons.reforatec.org/care-types/riego.png",
                IsActive = true
            },
            new()
            {
                Name = "Poda",
                IconUrl = "https://icons.reforatec.org/care-types/poda.png",
                IsActive = true
            },
            new()
            {
                Name = "Fertilización",
                IconUrl = "https://icons.reforatec.org/care-types/fertilizacion.png",
                IsActive = true
            },
            new()
            {
                Name = "Inspección Médica",
                IconUrl = "https://icons.reforatec.org/care-types/inspeccion.png",
                IsActive = true
            },
            new()
            {
                Name = "Limpieza de Cajete",
                IconUrl = "https://icons.reforatec.org/care-types/limpieza.png",
                IsActive = true
            }
        };

        foreach (var careType in careTypesList)
        {
            careType.Normalize();
            context.CareTypes.Add(careType);
        }

        await context.SaveChangesAsync();
    }
}
