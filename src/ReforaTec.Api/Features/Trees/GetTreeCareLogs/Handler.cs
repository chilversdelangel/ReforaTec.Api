using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities.Enums;
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
        var (_, tenantId, userRole) = loggedUser;
        
        if (!await TreeExistsAsync(context, tenantId, treeId, cancellationToken))
        {
            return Errors.TreeNotFound(treeId);
        }

        if (userRole == UserRole.Student
            && !await IsStudentAssignedToTreeAsync(context, loggedUser, treeId, cancellationToken))
        {
            return Errors.UserNotAssignedToTree;
        }

        var careLogs = await GetCareLogsAsync(context, tenantId, treeId, cancellationToken);

        return new Response(careLogs);
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

    private static Task<bool> IsStudentAssignedToTreeAsync(
        AppDbContext context,
        UserIdentity student,
        int treeId,
        CancellationToken cancellationToken)
    {
        return context.CareAssignments
            .AsNoTracking()
            .Where(assignment => assignment.TenantId == student.TenantId)
            .Where(assignment => assignment.UserId == student.UserId)
            .Where(assignment => assignment.TreeId == treeId)
            .Where(assignment => assignment.EndDate == null)
            .AnyAsync(cancellationToken);
    }

    private static async Task<IReadOnlyList<CareLogDto>> GetCareLogsAsync(
        AppDbContext context,
        int tenantId,
        int treeId,
        CancellationToken cancellationToken)
    {
        return await context.CareLogs
            .AsNoTracking()
            .Where(careLog => careLog.TenantId == tenantId)
            .Where(careLog => careLog.TreeId == treeId)
            .OrderByDescending(careLog => careLog.PerformedAt)
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
    }
}
