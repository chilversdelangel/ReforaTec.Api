using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Seeding;

internal static partial class DbSeeder
{
    private static async Task SeedStudentAssignmentsAsync(AppDbContext context)
    {
        if (await context.UserCaresForTrees.AnyAsync())
        {
            return;
        }

        var user = await context.Users.FirstOrDefaultAsync(user => user.Email == "student@cdmadero.tecnm.mx");
        if (user is null)
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

        var campaign = await context.Campaigns
            .Where(campaign => campaign.TenantId == user.TenantId)
            .FirstOrDefaultAsync();

        var firstAssignment = new UserCaresForTree
        {
            TenantId = user.TenantId,
            UserId = user.Id,
            TreeId = trees[0].Id,
            CampaignId = campaign?.Id,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-6))
        };

        var secondAssignment = new UserCaresForTree
        {
            TenantId = user.TenantId,
            UserId = user.Id,
            TreeId = trees[1].Id,
            CampaignId = campaign?.Id,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1))
        };

        context.UserCaresForTrees.AddRange(firstAssignment, secondAssignment);

        await context.SaveChangesAsync();
    }
}
