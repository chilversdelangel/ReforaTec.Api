using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Common;
using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Database;

/// <summary>
/// Provides initial master catalog seed data for development and testing environments.
/// </summary>
internal static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await SeedTenantsAsync(context);
        await SeedUsersAsync(context);
        await SeedSpeciesAsync(context);
        await SeedValuesAsync(context);
        await SeedServiceTypesAsync(context);
        await SeedNotificationTemplatesAsync(context);
        await SeedTreesAsync(context);
        await SeedCampaignsAsync(context);
        await SeedTreesToCampaignAsync(context);
        await SeedStudentAssignmentsAsync(context);
        await SeedMeasurementsAsync(context);
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
        if (await context.Users.AnyAsync()) return;

        var tenant = await context.Tenants.FirstAsync(t => t.Acronym == "ITCM");

        var testUser = new User
        {
            TenantId = tenant.Id,
            CurrentRole = UserRole.Student,
            Email = "student@cdmadero.tecnm.mx",
            ControlNumber = "21070001",
            FirstName = "Juan",
            LastName = "Pérez"
        };
        testUser.Normalize();

        var deletedUser = new User
        {
            TenantId = tenant.Id,
            CurrentRole = UserRole.Student,
            Email = "deleted_student@cdmadero.tecnm.mx",
            ControlNumber = "21070002",
            FirstName = "Carlos",
            LastName = "Gómez",
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow
        };
        deletedUser.Normalize();

        context.Users.AddRange(testUser, deletedUser);
        await context.SaveChangesAsync();
    }

    private static async Task SeedTreesAsync(AppDbContext context)
    {
        if (await context.Trees.AnyAsync())
        {
            return;
        }

        var tenant = await context.Tenants.FirstAsync(t => t.Acronym == "ITCM");
        var speciesList = await context.Species.Take(2).ToListAsync();
        var value = await context.Values.FirstAsync();

        if (speciesList.Count < 2)
        {
            return;
        }

        var tree1 = new Tree
        {
            TenantId = tenant.Id,
            SpeciesId = speciesList[0].Id,
            ValueId = value.Id,
            HealthState = TreeHealthState.Healthy,
            PlantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
            HeightCentimeters = 150.5m,
            DiameterCentimeters = 15.2m,
            Notes = "Árbol veterano plantado cerca del edificio de sistemas."
        };

        var tree2 = new Tree
        {
            TenantId = tenant.Id,
            SpeciesId = speciesList[1].Id,
            ValueId = value.Id,
            HealthState = TreeHealthState.NeedsAttention,
            PlantingDate = null,
            HeightCentimeters = null,
            DiameterCentimeters = null,
            Notes = "Árbol recién registrado sin medidas iniciales."
        };

        context.Trees.AddRange(tree1, tree2);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCampaignsAsync(AppDbContext context)
    {
        if (await context.Campaigns.AnyAsync())
        {
            return;
        }

        var tenant = await context.Tenants.FirstAsync(t => t.Acronym == "ITCM");

        var campaign = new Campaign
        {
            TenantId = tenant.Id,
            CampaignName = "ReforaTec 2026-A",
            InscriptionCode = "ITCM-2026A",
            Period = new Period
            {
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-2)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(2))
            }
        };
        campaign.Normalize();

        context.Campaigns.Add(campaign);
        await context.SaveChangesAsync();
    }

    private static async Task SeedTreesToCampaignAsync(AppDbContext context)
    {
        if (await context.CampaignManagesTrees.AnyAsync())
        {
            return;
        }

        var campaign = await context.Campaigns.FirstOrDefaultAsync();
        if (campaign == null)
        {
            return;
        }

        var tree1 = await context.Trees
            .Where(t => t.TenantId == campaign.TenantId)
            .OrderBy(t => t.Id)
            .FirstOrDefaultAsync();

        if (tree1 == null)
        {
            return;
        }

        var campaignLink = new CampaignManagesTree
        {
            TenantId = campaign.TenantId,
            CampaignId = campaign.Id,
            TreeId = tree1.Id,
            CampaignFolio = "01",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1))
        };
        campaignLink.Normalize();
        
        context.CampaignManagesTrees.Add(campaignLink);
        await context.SaveChangesAsync();
    }

    private static async Task SeedStudentAssignmentsAsync(AppDbContext context)
    {
        if (await context.UserCaresForTrees.AnyAsync())
        {
            return;
        }

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "student@cdmadero.tecnm.mx");
        if (user == null)
        {
            return;
        }

        var trees = await context.Trees
            .Where(t => t.TenantId == user.TenantId)
            .OrderBy(t => t.Id)
            .Take(2)
            .ToListAsync();

        if (trees.Count < 2)
        {
            return;
        }

        var care1 = new UserCaresForTree
        {
            TenantId = user.TenantId,
            UserId = user.Id,
            TreeId = trees[0].Id,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-6))
        };

        var care2 = new UserCaresForTree
        {
            TenantId = user.TenantId,
            UserId = user.Id,
            TreeId = trees[1].Id,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1))
        };
        
        context.UserCaresForTrees.Add(care1);
        context.UserCaresForTrees.Add(care2);
        
        await context.SaveChangesAsync();
    }

    private static async Task SeedMeasurementsAsync(AppDbContext context)
    {
        if (await context.Measurements.AnyAsync())
        {
            return;
        }

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "student@cdmadero.tecnm.mx");
        if (user == null)
        {
            return;
        }

        var tree = await context.Trees
            .Where(t => t.TenantId == user.TenantId)
            .OrderBy(t => t.Id)
            .FirstOrDefaultAsync();

        var campaign = await context.Campaigns
            .Where(c => c.TenantId == user.TenantId)
            .FirstOrDefaultAsync();

        if (tree == null || campaign == null)
        {
            return;
        }

        var measurement1 = new Measurement
        {
            TenantId = user.TenantId,
            TreeId = tree.Id,
            InspectorId = user.Id,
            CampaignId = campaign.Id,
            InspectionType = MeasurementType.Baseline,
            DetectedHealthState = TreeHealthState.Healthy,
            HeightCentimeters = 145m,
            DiameterCentimeters = 14.5m,
            EvidencePhotoUrl = "https://images.reforatec.org/evidence/tree1_baseline.jpg",
            DeviceCapturedAt = DateTime.UtcNow.AddMonths(-1)
        };

        var measurement2 = new Measurement
        {
            TenantId = user.TenantId,
            TreeId = tree.Id,
            InspectorId = user.Id,
            CampaignId = campaign.Id,
            InspectionType = MeasurementType.MidCampaign,
            DetectedHealthState = TreeHealthState.Healthy,
            HeightCentimeters = 150.5m,
            DiameterCentimeters = 15.2m,
            EvidencePhotoUrl = "https://images.reforatec.org/evidence/tree1_latest.jpg",
            DeviceCapturedAt = DateTime.UtcNow.AddDays(-2)
        };

        context.Measurements.Add(measurement1);
        context.Measurements.Add(measurement2);
        
        await context.SaveChangesAsync();
    }
}