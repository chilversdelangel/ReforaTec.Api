using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Seeding;

internal static partial class DbSeeder
{
    private static async Task SeedTreesToCampaignAsync(AppDbContext context)
    {
        if (await context.CampaignManagesTrees.AnyAsync())
        {
            return;
        }

        var campaign = await context.Campaigns.FirstOrDefaultAsync();
        if (campaign is null)
        {
            return;
        }

        var tree = await context.Trees
            .Where(tree => tree.TenantId == campaign.TenantId)
            .OrderBy(tree => tree.Id)
            .FirstOrDefaultAsync();

        if (tree is null)
        {
            return;
        }

        var campaignLink = new CampaignManagesTree
        {
            TenantId = campaign.TenantId,
            CampaignId = campaign.Id,
            TreeId = tree.Id,
            CampaignFolio = "01",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1))
        };
        campaignLink.Normalize();

        context.CampaignManagesTrees.Add(campaignLink);
        await context.SaveChangesAsync();
    }
}
