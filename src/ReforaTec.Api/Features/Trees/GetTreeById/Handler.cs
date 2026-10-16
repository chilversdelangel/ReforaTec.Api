using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Features.Trees.GetTreeById;

internal static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        int id,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var tree = await FindTreeWithDetailsAsync(context, id, cancellationToken);
        if (tree is null)
        {
            return Errors.TreeNotFound;
        }

        var currentFolio = await GetActiveCampaignFolioAsync(context, id, cancellationToken);
        var latestPhotoUrl = await GetLatestEvidencePhotoUrlAsync(context, id, cancellationToken);
        var assignedStudents = await GetAssignedStudentsAsync(context, id, cancellationToken);

        return MapToResponse(tree, currentFolio, latestPhotoUrl, assignedStudents);
    }

    private static Task<Tree?> FindTreeWithDetailsAsync(
        AppDbContext context,
        int treeId,
        CancellationToken cancellationToken)
    {
        return context.Trees
            .AsNoTracking()
            .Include(tree => tree.Species)
            .Include(tree => tree.Value)
            .FirstOrDefaultAsync(tree => tree.Id == treeId, cancellationToken);
    }

    private static Task<string?> GetActiveCampaignFolioAsync(
        AppDbContext context,
        int treeId,
        CancellationToken cancellationToken)
    {
        return context.CampaignManagesTrees
            .AsNoTracking()
            .Where(campaign => campaign.TreeId == treeId)
            .Where(campaign => campaign.EndDate == null)
            .Select(campaign => campaign.CampaignFolio)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static Task<string?> GetLatestEvidencePhotoUrlAsync(
        AppDbContext context,
        int treeId,
        CancellationToken cancellationToken)
    {
        return context.Measurements
            .AsNoTracking()
            .Where(measurement => measurement.TreeId == treeId)
            .Where(measurement => measurement.EvidencePhotoUrl != null)
            .OrderByDescending(measurement => measurement.DeviceCapturedAt)
            .Select(measurement => measurement.EvidencePhotoUrl)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static Task<List<AssignedStudentDto>> GetAssignedStudentsAsync(
        AppDbContext context,
        int treeId,
        CancellationToken cancellationToken)
    {
        return context.UserCaresForTrees
            .AsNoTracking()
            .Where(assignment => assignment.TreeId == treeId)
            .Where(assignment => assignment.EndDate == null)
            .Where(assignment => assignment.User != null && !assignment.User.IsDeleted)
            .Select(assignment => new AssignedStudentDto(
                assignment.User!.Id,
                assignment.User.ControlNumber,
                assignment.User.FirstName,
                assignment.User.LastName,
                assignment.User.SecondLastName,
                assignment.User.DisplayName))
            .ToListAsync(cancellationToken);
    }

    private static Response MapToResponse(
        Tree tree,
        string? currentFolio,
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
            CurrentCampaignFolio: currentFolio,
            HealthState: tree.HealthState.ToString(),
            PlantingDate: tree.PlantingDate,
            Observations: tree.Notes,
            Value: tree.Value!.ValueName,
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

