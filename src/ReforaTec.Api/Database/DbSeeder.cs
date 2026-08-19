using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Database;

/// <summary>
/// Provides initial master catalog seed data for development and testing environments.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await SeedTenantsAsync(context);
        await SeedUsersAsync(context);
        await SeedSpeciesAsync(context);
        await SeedValuesAsync(context);
        await SeedServiceTypesAsync(context);
        await SeedNotificationTemplatesAsync(context);
    }

    private static async Task SeedTenantsAsync(AppDbContext context)
    {
        if (await context.Tenants.AnyAsync()) return;

        var defaultTenant = new Tenant
        {
            InstitutionName = "Instituto Tecnológico de Ciudad Madero",
            Acronym = "ITCM",
            InstitutionalEmailDomain = "cdmadero.tecnm.mx"
        };
        defaultTenant.Normalize();

        context.Tenants.Add(defaultTenant);
        await context.SaveChangesAsync();
    }

    private static async Task SeedSpeciesAsync(AppDbContext context)
    {
        if (await context.Species.AnyAsync()) return;

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

    private static async Task SeedValuesAsync(AppDbContext context)
    {
        if (await context.Values.AnyAsync()) return;

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

    private static async Task SeedServiceTypesAsync(AppDbContext context)
    {
        if (await context.ServiceTypes.AnyAsync()) return;

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

        foreach (var st in serviceTypesList)
        {
            st.Normalize();
            context.ServiceTypes.Add(st);
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedNotificationTemplatesAsync(AppDbContext context)
    {
        if (await context.NotificationTemplates.AnyAsync()) return;

        var notificationTemplates = new List<NotificationTemplate>
        {
            new()
            {
                Type = NotificationType.TreeHealthAlert,
                TargetHealthState = TreeHealthState.Critical,
                Title = "⚠️ Árbol en Estado Crítico",
                MessageBody = "El árbol asignado requiere atención inmediata por desecación o plaga.",
                IsActive = true
            },
            new()
            {
                Type = NotificationType.WateringReminder,
                TargetHealthState = null,
                Title = "💧 Recordatorio de Riego",
                MessageBody = "Recuerda registrar el riego semanal de tu árbol asignado esta semana.",
                IsActive = true
            }
        };

        context.NotificationTemplates.AddRange(notificationTemplates);
        await context.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(AppDbContext context)
    {
        var tenant = await context.Tenants.FirstAsync();

        var usersToSeed = new List<User>
        {
            new()
            {
                TenantId = tenant.Id,
                CurrentRole = UserRole.SystemAdmin,
                Email = "admin@cdmadero.tecnm.mx",
                ControlNumber = "ADM0001",
                FirstName = "Admin",
                LastName = "General"
            },
            new()
            {
                TenantId = tenant.Id,
                CurrentRole = UserRole.Coordinator,
                Email = "coordinador@cdmadero.tecnm.mx",
                ControlNumber = "CRD0001",
                FirstName = "Danna",
                LastName = "Coordinadora"
            },
            new()
            {
                TenantId = tenant.Id,
                CurrentRole = UserRole.Student,
                Email = "student@cdmadero.tecnm.mx",
                ControlNumber = "21070001",
                FirstName = "Juan",
                LastName = "Pérez"
            },
            new()
            {
                TenantId = tenant.Id,
                CurrentRole = UserRole.Student,
                Email = "deleted_student@cdmadero.tecnm.mx",
                ControlNumber = "21070002",
                FirstName = "Carlos",
                LastName = "Gómez",
                IsDeleted = true,
                DeletedAt = DateTime.UtcNow
            }
        };

        foreach (var user in usersToSeed)
        {
            user.Normalize();
            var exists = await context.Users.AnyAsync(u => u.Email == user.Email);
            if (!exists)
            {
                context.Users.Add(user);
            }
        }

        await context.SaveChangesAsync();
    }
}
