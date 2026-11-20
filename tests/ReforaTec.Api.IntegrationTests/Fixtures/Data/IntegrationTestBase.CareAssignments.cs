using Microsoft.Extensions.DependencyInjection;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.IntegrationTests.Fixtures;

public abstract partial class IntegrationTestBase
{
    protected async Task<CareAssignment> CreateCareAssignmentAsync(
        int treeId,
        int userId,
        int? campaignId = null,
        int tenantId = TestTenants.Primary,
        DateOnly? startDate = null,
        DateOnly? endDate = null)
    {
        var resolvedCampaignId = campaignId ?? (await CreateCampaignAsync(tenantId: tenantId)).Id;

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var assignment = new CareAssignment
        {
            TenantId = tenantId,
            CampaignId = resolvedCampaignId,
            TreeId = treeId,
            UserId = userId,
            StartDate = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = endDate
        };

        dbContext.CareAssignments.Add(assignment);
        await dbContext.SaveChangesAsync();

        return assignment;
    }
}
