using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.Trees.CreateTreeMeasurement;

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

        var measurement = new Measurement
        {
            TenantId = loggedUser.TenantId,
            TreeId = treeId,
            UserId = loggedUser.UserId,
            HeightCentimeters = request.HeightCentimeters,
            DeviceCapturedAt = request.DeviceCapturedAt.UtcDateTime
        };

        context.Measurements.Add(measurement);
        await context.SaveChangesAsync(cancellationToken);

        return new Response(
            Id: measurement.Id,
            TreeId: treeId,
            HeightCentimeters: measurement.HeightCentimeters,
            DeviceCapturedAt: measurement.DeviceCapturedAt,
            CreatedAt: measurement.CreatedAt);
    }
}
