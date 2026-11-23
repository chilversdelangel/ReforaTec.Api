using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Features.Common.Dtos;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.CampaignEnrollments.EnrollInCampaign;

internal static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        UserIdentity loggedUser,
        Request request,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var sanitizedCode = request.CampaignCode.ToSanitized();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var normalizedCode = sanitizedCode.ToNormalized();

        var campaign = await GetCampaignByCodeAsync(
            context,
            loggedUser.TenantId,
            normalizedCode,
            cancellationToken);

        if (campaign is null)
        {
            return Errors.CampaignNotFound(sanitizedCode);
        }

        if (campaign.Period.StartDate > today || campaign.Period.EndDate < today)
        {
            return Errors.CampaignNotActive;
        }

        if (await IsAlreadyEnrolledAsync(context, campaign.Id, loggedUser.UserId, cancellationToken))
        {
            return Errors.AlreadyEnrolled;
        }

        var enrollment = new CampaignEnrollment
        {
            TenantId = loggedUser.TenantId,
            CampaignId = campaign.Id,
            UserId = loggedUser.UserId
        };

        context.CampaignEnrollments.Add(enrollment);
        await context.SaveChangesAsync(cancellationToken);

        return new Response(
            Id: enrollment.Id,
            CampaignId: campaign.Id,
            CampaignCode: campaign.Code,
            Period: new PeriodDto(campaign.Period.StartDate, campaign.Period.EndDate));
    }

    private static Task<Campaign?> GetCampaignByCodeAsync(
        AppDbContext context,
        int tenantId,
        string normalizedCode,
        CancellationToken cancellationToken) =>
        context.Campaigns
            .AsNoTracking()
            .Where(campaign => campaign.TenantId == tenantId)
            .Where(campaign => campaign.NormalizedCode == normalizedCode)
            .FirstOrDefaultAsync(cancellationToken);

    private static Task<bool> IsAlreadyEnrolledAsync(
        AppDbContext context,
        int campaignId,
        int userId,
        CancellationToken cancellationToken) =>
        context.CampaignEnrollments
            .AsNoTracking()
            .Where(enrollment => enrollment.CampaignId == campaignId)
            .Where(enrollment => enrollment.UserId == userId)
            .AnyAsync(cancellationToken);
}
