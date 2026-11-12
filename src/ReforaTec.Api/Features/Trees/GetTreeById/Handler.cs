using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
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
            return Errors.TreeNotFound;
        }

        var latestPhotoUrl = await GetLatestEvidencePhotoUrlAsync(context, tenantId, treeId, cancellationToken);
        var assignedStudents = await GetAssignedStudentsAsync(context, tenantId, treeId, cancellationToken);

        return MapToResponse(tree, latestPhotoUrl, assignedStudents);
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

    private static Task<string?> GetLatestEvidencePhotoUrlAsync(
        AppDbContext context,
        int tenantId,
        int treeId,
        CancellationToken cancellationToken)
    {
        return context.Measurements
            .AsNoTracking()
            .Where(measurement => measurement.TenantId == tenantId)
            .Where(measurement => measurement.TreeId == treeId)
            .Where(measurement => measurement.EvidencePhotoUrl != null)
            .OrderByDescending(measurement => measurement.DeviceCapturedAt)
            .Select(measurement => measurement.EvidencePhotoUrl)
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
            .Where(assignment => !assignment.User!.IsDeleted)
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
        string? latestPhotoUrl,
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
            
            HealthState: tree.HealthState.ToString(),
            PlantingDate: tree.PlantingDate,
            Observations: tree.Notes,
            
            Virtue: tree.Virtue!.Name,
            Species: tree.Species!.CommonName,
            SpeciesScientificName: tree.Species.ScientificName,
            SpeciesImageUrl: tree.Species.ImageUrl,
            
            CurrentHeightCentimeters: tree.HeightCentimeters,
            CurrentDiameterCentimeters: tree.DiameterCentimeters,
            LatestPhotoUrl: latestPhotoUrl,
            
            Location: locationDto,
            CurrentAssignedStudents: assignedStudents);
    }
}

