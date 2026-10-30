using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Seeding;

internal static partial class DbSeeder
{
    private static async Task SeedTenantsAsync(AppDbContext context)
    {
        if (await context.Tenants.AnyAsync())
        {
            return;
        }

        var defaultTenant = new Tenant
        {
            InstitutionName = "Instituto Tecnológico de Ciudad Madero",
            Acronym = "ITCM",
            InstitutionalEmailDomain = "cdmadero.tecnm.mx",
        };
        defaultTenant.Normalize();

        context.Tenants.Add(defaultTenant);
        await context.SaveChangesAsync();
    }
}
