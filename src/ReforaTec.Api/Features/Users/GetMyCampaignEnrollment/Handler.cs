using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.Users.GetMyCampaignEnrollment;

internal static class Handler
{
    public static async Task<Response> Handle(
        UserIdentity loggedUser,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var isEnrolled = await context.CampaignEnrollments
            .AsNoTracking()
            .Where(enrollment => enrollment.TenantId == loggedUser.TenantId)
            .Where(enrollment => enrollment.UserId == loggedUser.UserId)
            .Select(enrollment => enrollment.Campaign!.Period)
            .Where(period => period.StartDate <= today)
            .Where(period => period.EndDate >= today || period.EndDate == null)
            .AnyAsync(cancellationToken);

        return new Response(isEnrolled);
    }
}
