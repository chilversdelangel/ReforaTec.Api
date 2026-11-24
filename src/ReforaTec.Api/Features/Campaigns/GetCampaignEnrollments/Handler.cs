using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.Campaigns.GetCampaignEnrollments;

internal static class Handler
{
    public static async Task<ErrorOr<IReadOnlyList<Response>>> Handle(
        UserIdentity loggedUser,
        int campaignId,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        if (loggedUser.Role is not (UserRole.Coordinator or UserRole.SystemAdmin))
        {
            return Errors.Forbidden;
        }

        if (!await CampaignExistsAsync(context, loggedUser.TenantId, campaignId, cancellationToken))
        {
            return Errors.CampaignNotFound(campaignId);
        }

        var enrollments = await GetEnrollmentsAsync(context, loggedUser.TenantId, campaignId, cancellationToken);
        return ErrorOrFactory.From(enrollments);
    }

    private static Task<bool> CampaignExistsAsync(
        AppDbContext context,
        int tenantId,
        int campaignId,
        CancellationToken cancellationToken)
    {
        return context.Campaigns
            .AsNoTracking()
            .Where(campaign => campaign.TenantId == tenantId)
            .Where(campaign => campaign.Id == campaignId)
            .AnyAsync(cancellationToken);
    }

    private static async Task<IReadOnlyList<Response>> GetEnrollmentsAsync(
        AppDbContext context,
        int tenantId,
        int campaignId,
        CancellationToken cancellationToken)
    {
        return await context.CampaignEnrollments
            .AsNoTracking()
            .Where(enrollment => enrollment.TenantId == tenantId)
            .Where(enrollment => enrollment.CampaignId == campaignId)
            .OrderBy(enrollment => enrollment.CreatedAt)
            .Select(enrollment => new Response(
                enrollment.Id,
                new UserSummary(
                    enrollment.UserId,
                    enrollment.User!.GivenName,
                    enrollment.User!.FamilyName,
                    enrollment.User!.SecondFamilyName,
                    enrollment.User!.DisplayName,
                    enrollment.User!.Email,
                    enrollment.User!.ControlNumber),
                enrollment.IsApproved,
                enrollment.EvaluationNotes,
                enrollment.CreatedAt,
                enrollment.IsApproved.HasValue ? enrollment.ModifiedAt : null))
            .ToListAsync(cancellationToken);
    }
}
