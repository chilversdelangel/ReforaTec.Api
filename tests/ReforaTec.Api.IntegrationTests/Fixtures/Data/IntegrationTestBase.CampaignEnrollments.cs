using Microsoft.Extensions.DependencyInjection;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;

// ReSharper disable once CheckNamespace
namespace ReforaTec.Api.IntegrationTests.Fixtures;

public abstract partial class IntegrationTestBase
{
    protected async Task<CampaignEnrollment> EnrollUserInCampaignAsync(
        int campaignId,
        int userId,
        int tenantId = 1,
        bool? isApproved = null,
        string? evaluationNotes = null)
    {
        using var scope = Factory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var enrollment = new CampaignEnrollment
        {
            TenantId = tenantId,
            CampaignId = campaignId,
            UserId = userId,
            IsApproved = isApproved,
            EvaluationNotes = evaluationNotes
        };

        appDbContext.CampaignEnrollments.Add(enrollment);
        await appDbContext.SaveChangesAsync();

        return enrollment;
    }
}
