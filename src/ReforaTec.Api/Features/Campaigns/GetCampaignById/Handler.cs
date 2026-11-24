using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Features.Common.Dtos;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.Campaigns.GetCampaignById;

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

        var campaign = await context.Campaigns
            .AsNoTracking()
            .Where(campaign => campaign.TenantId == loggedUser.TenantId)
            .Where(campaign => campaign.Id == campaignId)
            .Select(campaign => new Response(
                campaign.Id,
                campaign.Code,
                new PeriodDto(campaign.Period.StartDate, campaign.Period.EndDate)))
            .FirstOrDefaultAsync(cancellationToken);

        return campaign is null
            ? Errors.CampaignNotFound(campaignId)
            : campaign;
    }
}
