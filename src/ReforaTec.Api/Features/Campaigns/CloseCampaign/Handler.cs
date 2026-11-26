using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Features.Common.Dtos;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.Campaigns.CloseCampaign;

internal static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        UserIdentity loggedUser,
        int campaignId,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        if (loggedUser.Role is not (UserRole.Coordinator or UserRole.SystemAdmin))
        {
            return Errors.Forbidden;
        }

        var campaign = await FindCampaignAsync(context, loggedUser.TenantId, campaignId, cancellationToken);
        if (campaign is null)
        {
            return Errors.CampaignNotFound(campaignId);
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var activeAssignments = await GetActiveAssignmentsAsync(
            context,
            loggedUser.TenantId,
            campaignId,
            cancellationToken);

        // Allow closing an expired campaign if it still has active care assignments to sweep.
        if (campaign.Period.HasEndedBefore(today) && activeAssignments.Count == 0)
        {
            return Errors.AlreadyClosed;
        }

        if (campaign.Period.IsActiveOn(today))
        {
            campaign.Period.EndDate = today;
        }

        foreach (var assignment in activeAssignments)
        {
            assignment.EndDate = today < assignment.StartDate 
                ? assignment.StartDate 
                : today;
        }

        await context.SaveChangesAsync(cancellationToken);

        return new Response(
            campaign.Id,
            campaign.Code,
            new PeriodDto(campaign.Period.StartDate, campaign.Period.EndDate),
            activeAssignments.Count);
    }

    private static Task<Campaign?> FindCampaignAsync(
        AppDbContext context,
        int tenantId,
        int campaignId,
        CancellationToken cancellationToken)
    {
        return context.Campaigns
            .Where(campaign => campaign.TenantId == tenantId)
            .Where(campaign => campaign.Id == campaignId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static Task<List<CareAssignment>> GetActiveAssignmentsAsync(
        AppDbContext context,
        int tenantId,
        int campaignId,
        CancellationToken cancellationToken)
    {
        return context.CareAssignments
            .Where(assignment => assignment.TenantId == tenantId)
            .Where(assignment => assignment.CampaignId == campaignId)
            .Where(assignment => assignment.EndDate == null)
            .ToListAsync(cancellationToken);
    }
}
