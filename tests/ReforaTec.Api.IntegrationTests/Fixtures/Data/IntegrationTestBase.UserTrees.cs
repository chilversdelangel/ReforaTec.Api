using Microsoft.Extensions.DependencyInjection;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;

// ReSharper disable once CheckNamespace
namespace ReforaTec.Api.IntegrationTests.Fixtures;

public abstract partial class IntegrationTestBase
{
    protected async Task<UserCaresForTree> AssignTreeToUserAsync(
        int treeId,
        int userId,
        int? campaignId = null,
        int tenantId = 1,
        
        DateOnly? startDate = null,
        DateOnly? endDate = null)
    {
        using var scope = Factory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var assignment = new UserCaresForTree
        {
            TenantId = tenantId,
            UserId = userId,
            TreeId = treeId,
            CampaignId = campaignId,
            
            StartDate = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = endDate
        };

        appDbContext.UserCaresForTrees.Add(assignment);
        await appDbContext.SaveChangesAsync();

        return assignment;
    }
}
