using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Features.Common.Dtos;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.Campaigns.GetCampaigns;

internal static class Handler
{
    public static async Task<ErrorOr<IReadOnlyList<Response>>> Handle(
        UserIdentity loggedUser,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        if (loggedUser.Role is not (UserRole.Coordinator or UserRole.SystemAdmin))
        {
            return Errors.Forbidden;
        }

        var campaigns = await context.Campaigns
            .AsNoTracking()
            .Where(campaign => campaign.TenantId == loggedUser.TenantId)
            .OrderByDescending(campaign => campaign.Period.StartDate)
            .Select(campaign => new Response(
                campaign.Id,
                campaign.Code,
                new PeriodDto(campaign.Period.StartDate, campaign.Period.EndDate)))
            .ToListAsync(cancellationToken);

        return ErrorOrFactory.From<IReadOnlyList<Response>>(campaigns);
    }
}
