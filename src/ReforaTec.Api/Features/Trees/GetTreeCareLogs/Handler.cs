using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.Trees.GetTreeCareLogs;

internal static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        int treeId,
        UserIdentity loggedUser,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var tenantId = loggedUser.TenantId;

        var treeExists = await context.Trees
            .AsNoTracking()
            .Where(tree => tree.TenantId == tenantId)
            .Where(tree => tree.Id == treeId)
            .AnyAsync(cancellationToken);

        if (!treeExists)
        {
            return Errors.TreeNotFound(treeId);
        }

        var careLogs = await context.CareLogs
            .AsNoTracking()
            .Where(careLog => careLog.TenantId == tenantId)
            .Where(careLog => careLog.TreeId == treeId)
            .OrderByDescending(careLog => careLog.DeviceCapturedAt)
            .Select(careLog => new CareLogDto(
                careLog.Id,
                careLog.TreeId,
                careLog.CareTypeId,
                careLog.CareType!.Name,
                careLog.UserId,
                careLog.PerformedAt,
                careLog.DeviceCapturedAt,
                careLog.CreatedAt,
                careLog.Notes))
            .ToListAsync(cancellationToken);

        return new Response(careLogs);
    }
}
