using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.Users.GetMyTrees;

internal static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        UserIdentity loggedUser,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var (userId, tenantId) = loggedUser;

        var userExists = await UserExistsAsync(context, tenantId, userId, cancellationToken);
        if (!userExists)
        {
            return Errors.UserNotFound(userId);
        }

        var activeAssignments = 
            await GetActiveAssignmentsAsync(context, tenantId, userId, cancellationToken);
        if (activeAssignments.Count == 0)
        {
            return new Response([]);
        }

        var treeIds = activeAssignments
            .Select(assignment => assignment.TreeId)
            .Distinct()
            .ToList();

        var snapshotsByTreeId = 
            await GetTreeSnapshotsAsync(context, tenantId, treeIds, cancellationToken);

        var assignedTrees = activeAssignments
            .Select(assignment => MapToAssignedTreeDto(assignment, snapshotsByTreeId[assignment.TreeId]))
            .ToList();

        return new Response(assignedTrees);
    }

    private static Task<bool> UserExistsAsync(
        AppDbContext context,
        int tenantId,
        int userId,
        CancellationToken cancellationToken)
    {
        return context.Users
            .AsNoTracking()
            .Where(user => user.TenantId == tenantId)
            .Where(user => user.Id == userId)
            .Where(user => !user.IsDeleted)
            .AnyAsync(cancellationToken);
    }

    private static async Task<List<CareAssignment>> GetActiveAssignmentsAsync(
        AppDbContext context,
        int tenantId,
        int userId,
        CancellationToken cancellationToken)
    {
        return await context.CareAssignments
            .AsNoTracking()
            .Where(assignment => assignment.TenantId == tenantId)
            .Where(assignment => assignment.UserId == userId)
            .Where(assignment => assignment.EndDate == null)
            .Where(assignment => assignment.Tree != null)
            .Include(assignment => assignment.Tree)
            .ThenInclude(tree => tree!.Species)
            .Include(assignment => assignment.Tree)
            .ThenInclude(tree => tree!.Virtue)
            .OrderBy(assignment => assignment.TreeId)
            .ToListAsync(cancellationToken);
    }

    private static async Task<Dictionary<int, TreeSnapshot>> GetTreeSnapshotsAsync(
        AppDbContext context,
        int tenantId,
        List<int> treeIds,
        CancellationToken cancellationToken)
    {
        var measurements = 
            await GetLatestMeasurementsAsync(context, tenantId, treeIds, cancellationToken);
        var photos = 
            await GetLatestPhotosAsync(context, tenantId, treeIds, cancellationToken);
        var healthStates = 
            await GetLatestHealthStatesAsync(context, tenantId, treeIds, cancellationToken);

        return treeIds.ToDictionary(
            treeId => treeId,
            treeId =>
            {
                var measurement = measurements.GetValueOrDefault(treeId);

                return new TreeSnapshot(
                    measurement?.HeightCentimeters,
                    measurement?.DiameterCentimeters,
                    photos.GetValueOrDefault(treeId),
                    healthStates.GetValueOrDefault(treeId, TreeHealthState.Healthy));
            });
    }

    private static Task<Dictionary<int, LatestMeasurementSnapshot>> GetLatestMeasurementsAsync(
        AppDbContext context,
        int tenantId,
        List<int> treeIds,
        CancellationToken cancellationToken)
    {
        return context.Measurements
            .AsNoTracking()
            .Where(measurement => measurement.TenantId == tenantId)
            .Where(measurement => treeIds.Contains(measurement.TreeId))
            .GroupBy(measurement => measurement.TreeId)
            .Select(treeGroup => treeGroup
                .OrderByDescending(measurement => measurement.DeviceCapturedAt)
                .Select(measurement => new LatestMeasurementSnapshot(
                    measurement.TreeId,
                    measurement.HeightCentimeters,
                    measurement.DiameterCentimeters))
                .First())
            .ToDictionaryAsync(
                snapshot => snapshot.TreeId,
                cancellationToken);
    }

    private static Task<Dictionary<int, string?>> GetLatestPhotosAsync(
        AppDbContext context,
        int tenantId,
        List<int> treeIds,
        CancellationToken cancellationToken)
    {
        return context.Measurements
            .AsNoTracking()
            .Where(measurement => measurement.TenantId == tenantId)
            .Where(measurement => treeIds.Contains(measurement.TreeId))
            .Where(measurement => measurement.PhotoUrl != null)
            .GroupBy(measurement => measurement.TreeId)
            .Select(treeGroup => treeGroup
                .OrderByDescending(measurement => measurement.DeviceCapturedAt)
                .Select(measurement => new LatestPhotoSnapshot(
                    measurement.TreeId,
                    measurement.PhotoUrl))
                .First())
            .ToDictionaryAsync(
                snapshot => snapshot.TreeId,
                snapshot => snapshot.LatestPhotoUrl,
                cancellationToken);
    }

    private static Task<Dictionary<int, TreeHealthState>> GetLatestHealthStatesAsync(
        AppDbContext context,
        int tenantId,
        List<int> treeIds,
        CancellationToken cancellationToken)
    {
        return context.Inspections
            .AsNoTracking()
            .Where(inspection => inspection.TenantId == tenantId)
            .Where(inspection => treeIds.Contains(inspection.TreeId))
            .GroupBy(inspection => inspection.TreeId)
            .Select(treeGroup => treeGroup
                .OrderByDescending(inspection => inspection.DeviceCapturedAt)
                .Select(inspection => new LatestHealthStateSnapshot(
                    inspection.TreeId,
                    inspection.HealthState))
                .First())
            .ToDictionaryAsync(
                snapshot => snapshot.TreeId,
                snapshot => snapshot.HealthState,
                cancellationToken);
    }

    private static AssignedTreeDto MapToAssignedTreeDto(
        CareAssignment assignment,
        TreeSnapshot snapshot)
    {
        var tree = assignment.Tree!;

        return new AssignedTreeDto(
            Id: tree.Id,
            Folio: tree.Folio,

            SpeciesId: tree.SpeciesId,
            CommonName: tree.Species!.CommonName,
            ScientificName: tree.Species.ScientificName,
            SpeciesImageUrl: tree.Species.ImageUrl,

            VirtueName: tree.Virtue!.Name,

            HealthState: snapshot.HealthState.ToString(),
            PlantingDate: tree.PlantingDate,

            CurrentHeightCentimeters: snapshot.HeightCentimeters,
            CurrentDiameterCentimeters: snapshot.DiameterCentimeters,

            LatestPhotoUrl: snapshot.LatestPhotoUrl);
    }

    private sealed record TreeSnapshot(
        decimal? HeightCentimeters,
        decimal? DiameterCentimeters,
        string? LatestPhotoUrl,
        TreeHealthState HealthState);

    private sealed record LatestMeasurementSnapshot(
        int TreeId,
        decimal HeightCentimeters,
        decimal DiameterCentimeters);

    private sealed record LatestPhotoSnapshot(int TreeId, string? LatestPhotoUrl);

    private sealed record LatestHealthStateSnapshot(int TreeId, TreeHealthState HealthState);
}
