using Microsoft.Extensions.DependencyInjection;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Common;

// ReSharper disable once CheckNamespace
namespace ReforaTec.Api.IntegrationTests.Fixtures;

public abstract partial class IntegrationTestBase
{
    protected async Task<Campaign> CreateCampaignAsync(
        int tenantId = 1,
        
        string? campaignName = null,
        string? inscriptionCode = null,
        
        Period? period = null,
        Location? location = null)
    {
        using var scope = Factory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var campaign = new Campaign
        {
            TenantId = tenantId,

            CampaignName = campaignName ?? $"Campaign {Guid.NewGuid().ToString("N")[..8]}",
            InscriptionCode = inscriptionCode ?? $"C-{Guid.NewGuid().ToString("N")[..8]}",

            Period = period ?? new Period
            {
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
                EndDate = null
            },
            Location = location
        };

        campaign.Normalize();

        appDbContext.Campaigns.Add(campaign);
        await appDbContext.SaveChangesAsync();

        return campaign;
    }
}
