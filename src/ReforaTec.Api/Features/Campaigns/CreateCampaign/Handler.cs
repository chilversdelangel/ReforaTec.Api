using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Common;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Features.Common.Dtos;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.Campaigns.CreateCampaign;

internal static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        UserIdentity loggedUser,
        Request request,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        if (loggedUser.Role is not (UserRole.Coordinator or UserRole.SystemAdmin))
        {
            return Errors.Forbidden;
        }

        var normalizedCode = request.Code.ToNormalized();
        if (await CodeExistsAsync(context, loggedUser.TenantId, normalizedCode, cancellationToken))
        {
            return Errors.CodeAlreadyExists(request.Code.ToSanitized());
        }

        var campaign = new Campaign
        {
            TenantId = loggedUser.TenantId,
            Code = request.Code,
            Period = new Period
            {
                StartDate = request.Period.StartDate,
                EndDate = request.Period.EndDate
            }
        };

        context.Campaigns.Add(campaign);
        await context.SaveChangesAsync(cancellationToken);

        return new Response(
            Id: campaign.Id,
            Code: campaign.Code,
            Period: new PeriodDto(campaign.Period.StartDate, campaign.Period.EndDate));
    }

    private static Task<bool> CodeExistsAsync(
        AppDbContext context,
        int tenantId,
        string normalizedCode,
        CancellationToken cancellationToken) =>
        context.Campaigns
            .AsNoTracking()
            .Where(campaign => campaign.TenantId == tenantId)
            .Where(campaign => campaign.NormalizedCode == normalizedCode)
            .AnyAsync(cancellationToken);
}
