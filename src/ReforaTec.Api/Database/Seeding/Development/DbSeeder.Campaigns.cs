using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Database.Seeding;

internal static partial class DbSeeder
{
    private static async Task SeedCampaignsAsync(AppDbContext context)
    {
        if (await context.Campaigns.AnyAsync())
        {
            return;
        }

        var tenant = await context.Tenants.FirstAsync(tenant => tenant.Acronym == "ITCM");

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
}
