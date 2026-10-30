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
}
