using ErrorOr;
using Mapster;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.Trees.CreateTreeCareLog;

internal static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        int treeId,
        UserIdentity user,
        Request request,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var treeExists = await context.Trees
            .Where(tree => tree.Id == treeId)
            .Where(tree => tree.TenantId == user.TenantId)
            .AnyAsync(cancellationToken);

        if (!treeExists)
        {
            return Errors.TreeNotFound(treeId);
        }

        var isAssigned = await context.CareAssignments
            .Where(assignment => assignment.TenantId == user.TenantId)
            .Where(assignment => assignment.UserId == user.UserId)
            .Where(assignment => assignment.TreeId == treeId)
            .Where(assignment => assignment.EndDate == null)
            .AnyAsync(cancellationToken);

        if (!isAssigned)
        {
            return Errors.UserNotAssignedToTree;
        }

        var careTypeExists = await context.CareTypes
            .Where(careType => careType.Id == request.CareTypeId)
            .AnyAsync(cancellationToken);

        if (!careTypeExists)
        {
            return Errors.CareTypeNotFound(request.CareTypeId);
        }

        var careLog = new CareLog
        {
            TenantId = user.TenantId,
            UserId = user.UserId,
            TreeId = treeId,
            CareTypeId = request.CareTypeId,
            PerformedAt = request.PerformedAt.UtcDateTime,
            DeviceCapturedAt = request.DeviceCapturedAt.UtcDateTime,
            Notes = request.Notes
        };

        context.CareLogs.Add(careLog);
        await context.SaveChangesAsync(cancellationToken);

        return careLog.Adapt<Response>();
    }
}
