using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Enums;

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

        var campaign = await context.Campaigns
            .Where(campaign => campaign.TenantId == user.TenantId)
            .FirstOrDefaultAsync();

        if (tree is null || campaign is null)
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
