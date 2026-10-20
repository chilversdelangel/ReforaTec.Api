using Microsoft.Extensions.DependencyInjection;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;

// ReSharper disable once CheckNamespace
namespace ReforaTec.Api.IntegrationTests.Fixtures;

public abstract partial class IntegrationTestBase
{
    protected async Task<Service> CreateTreeServiceAsync(
        int treeId,
        int studentId,
        int serviceTypeId = 1,
        int? campaignId = null,
        int tenantId = 1,
        
        DateTime? deviceCapturedAt = null,
        DateTime? serverSyncedAt = null,
        string? comment = null)
    {
        using var scope = Factory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTime.UtcNow;

        var service = new Service
        {
            TenantId = tenantId,
            TreeId = treeId,
            StudentId = studentId,
            ServiceTypeId = serviceTypeId,
            CampaignId = campaignId,
            
            DeviceCapturedAt = deviceCapturedAt ?? now,
            ServerSyncedAt = serverSyncedAt ?? now,
            Comment = comment
        };

        appDbContext.Services.Add(service);
        await appDbContext.SaveChangesAsync();

        return service;
    }
}
