using Microsoft.Extensions.DependencyInjection;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;

// ReSharper disable once CheckNamespace
namespace ReforaTec.Api.IntegrationTests.Fixtures;

public abstract partial class IntegrationTestBase
{
    protected async Task<CampaignManagesTree> AddTreeToCampaignAsync(
        int treeId,
        int campaignId,
        int tenantId = 1,
        
        string? campaignFolio = null,
        
        DateOnly? startDate = null,
        DateOnly? endDate = null)
    {
        using var scope = Factory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var campaignTree = new CampaignManagesTree
        {
            TenantId = tenantId,
            CampaignId = campaignId,
            TreeId = treeId,
            
            CampaignFolio = campaignFolio ?? $"{Random.Shared.Next(1, 1000):D3}",
            
            StartDate = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = endDate
        };

        campaignTree.Normalize();

        appDbContext.CampaignManagesTrees.Add(campaignTree);
        await appDbContext.SaveChangesAsync();

        return campaignTree;
    }
}
