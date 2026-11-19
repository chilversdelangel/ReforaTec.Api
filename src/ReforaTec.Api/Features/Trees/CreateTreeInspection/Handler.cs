using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.Trees.CreateTreeInspection;

internal static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        int treeId,
        UserIdentity loggedUser,
        Request request,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        if (loggedUser.Role == UserRole.Student)
        {
            return Errors.InsufficientPermissions;
        }

        var treeExists = await context.Trees
            .Where(tree => tree.TenantId == loggedUser.TenantId)
            .Where(tree => tree.Id == treeId)
            .AnyAsync(cancellationToken);

        if (!treeExists)
        {
            return Errors.TreeNotFound(treeId);
        }

        var healthState = Enum.Parse<TreeHealthState>(request.HealthState, ignoreCase: true);

        var inspection = new Inspection
        {
            TenantId = loggedUser.TenantId,
            TreeId = treeId,
            UserId = loggedUser.UserId,
            HealthState = healthState,
            Notes = request.Notes,
            DeviceCapturedAt = request.DeviceCapturedAt.UtcDateTime
        };

        context.Inspections.Add(inspection);
        await context.SaveChangesAsync(cancellationToken);

        return new Response(
            Id: inspection.Id,
            TreeId: treeId,
            HealthState: inspection.HealthState.ToString(),
            Notes: inspection.Notes,
            DeviceCapturedAt: inspection.DeviceCapturedAt,
            CreatedAt: inspection.CreatedAt);
    }
}
