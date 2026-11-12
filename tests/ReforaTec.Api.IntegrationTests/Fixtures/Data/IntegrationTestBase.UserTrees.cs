using Microsoft.Extensions.DependencyInjection;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;

// ReSharper disable once CheckNamespace
namespace ReforaTec.Api.IntegrationTests.Fixtures;

public abstract partial class IntegrationTestBase
{
    protected async Task<CareAssignment> AssignTreeToUserAsync(
        int treeId,
        int userId,
        int campaignId,
        int tenantId = 1,
        
        DateOnly? startDate = null,
        DateOnly? endDate = null)
    {
        using var scope = Factory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var assignment = new CareAssignment
        {
            TenantId = tenantId,
            CampaignId = campaignId,
            UserId = userId,
            TreeId = treeId,
            
            StartDate = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = endDate
        };

        appDbContext.CareAssignments.Add(assignment);
        await appDbContext.SaveChangesAsync();

        return assignment;
    }
}
