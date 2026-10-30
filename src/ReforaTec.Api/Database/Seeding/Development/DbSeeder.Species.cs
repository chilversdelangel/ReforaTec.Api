using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Seeding;

internal static partial class DbSeeder
{
    private static async Task SeedSpeciesAsync(AppDbContext context)
    {
        if (await context.Species.AnyAsync())
        {
            return;
        }

        var speciesList = new List<Species>
        {
            new()
            {
                ScientificName = "Quercus virginiana",
                CommonName = "Encino Siempreverde",
                Description = "Árbol frondoso de madera dura, highly resistant to warm sub-humid climates.",
                ImageUrl = "https://images.reforatec.org/species/quercus-virginiana.jpg"
            },
            new()
            {
                ScientificName = "Prosopis juliflora",
                CommonName = "Mezquite",
                Description = "Especie nativa leguminosa fijadora de nitrógeno en el suelo, de bajo consumo hídrico.",
                ImageUrl = "https://images.reforatec.org/species/prosopis-juliflora.jpg"
            },
            new()
            {
                ScientificName = "Fraxinus americana",
                CommonName = "Fresno",
                Description = "Árbol caducifolio de sombra densa y crecimiento rápido en zonas urbanas.",
                ImageUrl = "https://images.reforatec.org/species/fraxinus-americana.jpg"
            },
            new()
            {
                ScientificName = "Vachellia farnesiana",
                CommonName = "Huizache",
                Description = "Árbol espinoso nativo con flores amarillas aromáticas y alta tolerancia a la sequía.",
                ImageUrl = "https://images.reforatec.org/species/vachellia-farnesiana.jpg"
            },
            new()
            {
                ScientificName = "Jacaranda mimosifolia",
                CommonName = "Jacaranda",
                Description = "Árbol ornamental caducifolio de llamativa floración violeta en primavera.",
                ImageUrl = "https://images.reforatec.org/species/jacaranda-mimosifolia.jpg"
            }
        };

        foreach (var species in speciesList)
        {
            species.Normalize();
            context.Species.Add(species);
        }

        await context.SaveChangesAsync();
    }
}
