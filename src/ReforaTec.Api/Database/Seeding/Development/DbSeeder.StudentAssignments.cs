using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Seeding;

internal static partial class DbSeeder
{
    private static async Task SeedStudentAssignmentsAsync(AppDbContext context)
    {
        if (await context.CareAssignments.AnyAsync())
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

        var trees = await context.Trees
            .Where(tree => tree.TenantId == user.TenantId)
            .OrderBy(tree => tree.Id)
            .Take(2)
            .ToListAsync();

        if (trees.Count < 2)
        {
            return;
        }

        var firstAssignment = new CareAssignment
        {
            TenantId = user.TenantId,
            CampaignId = campaign.Id,
            UserId = user.Id,
            TreeId = trees[0].Id,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-6))
        };

        var secondAssignment = new CareAssignment
        {
            TenantId = user.TenantId,
            CampaignId = campaign.Id,
            UserId = user.Id,
            TreeId = trees[1].Id,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1))
        };

        context.CareAssignments.AddRange(firstAssignment, secondAssignment);

        await context.SaveChangesAsync();
    }
}
