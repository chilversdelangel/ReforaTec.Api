using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Seeding;

internal static partial class DbSeeder
{
    private static async Task SeedCampaignEnrollmentsAsync(AppDbContext context)
    {
        if (await context.CampaignEnrollments.AnyAsync())
        {
            return;
        }

        var user = await context.Users.FirstOrDefaultAsync(user => user.Email == "student@cdmadero.tecnm.mx");
        if (user is null)
        {
            return;
        }

        var campaign = await context.Campaigns
            .Where(campaign => campaign.TenantId == user.TenantId)
            .OrderBy(campaign => campaign.Id)
            .FirstOrDefaultAsync();

        if (campaign is null)
        {
            return;
        }

        var enrollment = new CampaignEnrollment
        {
            TenantId = user.TenantId,
            CampaignId = campaign.Id,
            UserId = user.Id
        };

        context.CampaignEnrollments.Add(enrollment);
        await context.SaveChangesAsync();
    }
}
