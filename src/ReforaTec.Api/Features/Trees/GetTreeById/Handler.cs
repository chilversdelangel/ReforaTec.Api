using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.Trees.GetTreeById;

internal static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        int treeId,
        UserIdentity loggedUser,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var tenantId = loggedUser.TenantId;

        var tree = await FindTreeWithDetailsAsync(context, tenantId, treeId, cancellationToken);
        if (tree is null)
        {
            return Errors.TreeNotFound(treeId);
        }

        var snapshot = await GetTreeSnapshotAsync(context, tenantId, treeId, cancellationToken);
        var assignedStudents = await GetAssignedStudentsAsync(context, tenantId, treeId, cancellationToken);

        return MapToResponse(tree, snapshot, assignedStudents);
    }

    private static Task<Tree?> FindTreeWithDetailsAsync(
        AppDbContext context,
        int tenantId,
        int treeId,
        CancellationToken cancellationToken)
    {
        return context.Trees
            .AsNoTracking()
            .Where(tree => tree.TenantId == tenantId)
            .Where(tree => tree.Id == treeId)
            .Include(tree => tree.Species)
            .Include(tree => tree.Virtue)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static async Task<TreeSnapshot> GetTreeSnapshotAsync(
        AppDbContext context,
        int tenantId,
        int treeId,
        CancellationToken cancellationToken)
    {
        var latestMeasurement = await GetLatestMeasurementAsync(context, tenantId, treeId, cancellationToken);
        var latestPhotoUrl = await GetLatestPhotoUrlAsync(context, tenantId, treeId, cancellationToken);
        var latestInspection = await GetLatestInspectionAsync(context, tenantId, treeId, cancellationToken);

        return new TreeSnapshot(
            latestMeasurement,
            latestPhotoUrl,
            latestInspection?.HealthState ?? TreeHealthState.Healthy,
            latestInspection?.Notes);
    }

    private static Task<decimal> GetLatestMeasurementAsync(
        AppDbContext context,
        int tenantId,
        int treeId,
        CancellationToken cancellationToken)
    {
        return context.Measurements
            .AsNoTracking()
            .Where(measurement => measurement.TenantId == tenantId)
            .Where(measurement => measurement.TreeId == treeId)
            .OrderByDescending(measurement => measurement.DeviceCapturedAt)
            .Select(measurement => measurement.HeightCentimeters)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static Task<string?> GetLatestPhotoUrlAsync(
        AppDbContext context,
        int tenantId,
        int treeId,
        CancellationToken cancellationToken)
    {
        return context.Measurements
            .AsNoTracking()
            .Where(measurement => measurement.TenantId == tenantId)
            .Where(measurement => measurement.TreeId == treeId)
            .Where(measurement => measurement.PhotoUrl != null)
            .OrderByDescending(measurement => measurement.DeviceCapturedAt)
            .Select(measurement => measurement.PhotoUrl)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static Task<LatestInspectionSnapshot?> GetLatestInspectionAsync(
        AppDbContext context,
        int tenantId,
        int treeId,
        CancellationToken cancellationToken)
    {
        return context.Inspections
            .AsNoTracking()
            .Where(inspection => inspection.TenantId == tenantId)
            .Where(inspection => inspection.TreeId == treeId)
            .OrderByDescending(inspection => inspection.DeviceCapturedAt)
            .Select(inspection => new LatestInspectionSnapshot(
                inspection.HealthState,
                inspection.Notes))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static Task<List<AssignedStudentDto>> GetAssignedStudentsAsync(
        AppDbContext context,
        int tenantId,
        int treeId,
        CancellationToken cancellationToken)
    {
        return context.CareAssignments
            .AsNoTracking()
            .Where(assignment => assignment.TenantId == tenantId)
            .Where(assignment => assignment.TreeId == treeId)
            .Where(assignment => assignment.EndDate == null)
            .Where(assignment => assignment.User != null)
            .Select(assignment => new AssignedStudentDto(
                assignment.User!.Id,
                assignment.User.ControlNumber,
                assignment.User.GivenName,
                assignment.User.FamilyName,
                assignment.User.SecondFamilyName,
                assignment.User.DisplayName))
            .ToListAsync(cancellationToken);
    }

    private static Response MapToResponse(
        Tree tree,
        TreeSnapshot snapshot,
        List<AssignedStudentDto> assignedStudents)
    {
        var locationDto = tree.Location is not null
            ? new LocationDto(
                tree.Location.Latitude,
                tree.Location.Longitude,
                tree.Location.Street,
                tree.Location.Neighborhood,
                tree.Location.StreetNumber)
            : null;

        return new Response(
            Id: tree.Id,
            Folio: tree.Folio,

            HealthState: snapshot.HealthState.ToString(),
            PlantingDate: tree.PlantingDate,
            Observations: snapshot.Notes,

            Virtue: tree.Virtue!.Name,
            Species: tree.Species!.CommonName,
            SpeciesScientificName: tree.Species.ScientificName,
            SpeciesImageUrl: tree.Species.ImageUrl,

            CurrentHeightCentimeters: snapshot.HeightCentimeters,
            LatestPhotoUrl: snapshot.LatestPhotoUrl,

            Location: locationDto,
            CurrentAssignedStudents: assignedStudents);
    }

    private sealed record TreeSnapshot(
        decimal? HeightCentimeters,
        string? LatestPhotoUrl,
        TreeHealthState HealthState,
        string? Notes);

    private sealed record LatestInspectionSnapshot(
        TreeHealthState HealthState,
        string? Notes);
}
