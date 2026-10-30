using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Seeding;

internal static partial class DbSeeder
{
    private static async Task SeedServiceTypesAsync(AppDbContext context)
    {
        if (await context.ServiceTypes.AnyAsync())
        {
            return;
        }

        var serviceTypesList = new List<ServiceType>
        {
            new()
            {
                ServiceName = "Riego",
                IconUrl = "https://icons.reforatec.org/services/riego.png",
                IsActive = true
            },
            new()
            {
                ServiceName = "Poda",
                IconUrl = "https://icons.reforatec.org/services/poda.png",
                IsActive = true
            },
            new()
            {
                ServiceName = "Fertilización",
                IconUrl = "https://icons.reforatec.org/services/fertilizacion.png",
                IsActive = true
            },
            new()
            {
                ServiceName = "Inspección Médica",
                IconUrl = "https://icons.reforatec.org/services/inspeccion.png",
                IsActive = true
            },
            new()
            {
                ServiceName = "Limpieza de Cajete",
                IconUrl = "https://icons.reforatec.org/services/limpieza.png",
                IsActive = true
            }
        };

        foreach (var serviceType in serviceTypesList)
        {
            serviceType.Normalize();
            context.ServiceTypes.Add(serviceType);
        }

        await context.SaveChangesAsync();
    }
}
