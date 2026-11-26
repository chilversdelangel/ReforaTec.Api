using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Common;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.Campaigns.CreateCareAssignment;

internal static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        UserIdentity loggedUser,
        int campaignId,
        Request request,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        if (loggedUser.Role is not (UserRole.Coordinator or UserRole.SystemAdmin))
        {
            return Errors.Forbidden;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var campaignPeriod = await GetCampaignPeriodAsync(
            context,
            loggedUser.TenantId,
            campaignId,
            cancellationToken);

        if (campaignPeriod is null)
        {
            return Errors.CampaignNotFound(campaignId);
        }

        if (!campaignPeriod.IsActiveOn(today))
        {
            return Errors.CampaignNotActive;
        }

        if (!await IsUserEnrolledAsync(context, loggedUser.TenantId, campaignId, request.UserId, cancellationToken))
        {
            return Errors.EnrollmentNotFound(request.UserId, campaignId);
        }

        if (!await TreeExistsAsync(context, loggedUser.TenantId, request.TreeId, cancellationToken))
        {
            return Errors.TreeNotFound(request.TreeId);
        }

        if (await HasActiveAssignmentAsync(context, campaignId, request.UserId, request.TreeId, cancellationToken))
        {
            return Errors.AlreadyAssigned;
        }

        var assignment = new CareAssignment
        {
            TenantId = loggedUser.TenantId,
            CampaignId = campaignId,
            UserId = request.UserId,
            TreeId = request.TreeId,
            StartDate = request.StartDate,
            EndDate = null
        };

        context.CareAssignments.Add(assignment);
        await context.SaveChangesAsync(cancellationToken);

        return new Response(
            assignment.Id,
            assignment.CampaignId,
            assignment.UserId,
            assignment.TreeId,
            assignment.StartDate,
            assignment.EndDate);
    }

    private static Task<Period?> GetCampaignPeriodAsync(
        AppDbContext context,
        int tenantId,
        int campaignId,
        CancellationToken cancellationToken)
    {
        return context.Campaigns
            .AsNoTracking()
            .Where(campaign => campaign.TenantId == tenantId)
            .Where(campaign => campaign.Id == campaignId)
            .Select(campaign => campaign.Period)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static Task<bool> IsUserEnrolledAsync(
        AppDbContext context,
        int tenantId,
        int campaignId,
        int userId,
        CancellationToken cancellationToken)
    {
        return context.CampaignEnrollments
            .AsNoTracking()
            .Where(enrollment => enrollment.TenantId == tenantId)
            .Where(enrollment => enrollment.CampaignId == campaignId)
            .Where(enrollment => enrollment.UserId == userId)
            .AnyAsync(cancellationToken);
    }

    private static Task<bool> TreeExistsAsync(
        AppDbContext context,
        int tenantId,
        int treeId,
        CancellationToken cancellationToken)
    {
        return context.Trees
            .AsNoTracking()
            .Where(tree => tree.TenantId == tenantId)
            .Where(tree => tree.Id == treeId)
            .AnyAsync(cancellationToken);
    }

    private static Task<bool> HasActiveAssignmentAsync(
        AppDbContext context,
        int campaignId,
        int userId,
        int treeId,
        CancellationToken cancellationToken)
    {
        return context.CareAssignments
            .AsNoTracking()
            .Where(assignment => assignment.CampaignId == campaignId)
            .Where(assignment => assignment.UserId == userId)
            .Where(assignment => assignment.TreeId == treeId)
            .Where(assignment => assignment.EndDate == null)
            .AnyAsync(cancellationToken);
    }
}
