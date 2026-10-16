using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;

namespace ReforaTec.Api.Features.Trees.GetTreeById;

internal static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        int id,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var tree = await context.Trees
            .AsNoTracking()
            .Include(tree => tree.Species)
            .Include(tree => tree.Value)
            .FirstOrDefaultAsync(tree => tree.Id == id, cancellationToken);

        if (tree is null)
        {
            return Error.NotFound(
                code: ErrorCodes.NotFound,
                description: $"Tree with ID {id} was not found.");
        }

        var currentFolio = await context.CampaignManagesTrees
            .AsNoTracking()
            .Where(campaign => campaign.TreeId == id)
            .Where(campaign => campaign.EndDate == null)
            .Select(campaign => campaign.CampaignFolio)
            .FirstOrDefaultAsync(cancellationToken);

        var latestPhoto = await context.Measurements
            .AsNoTracking()
            .Where(measurement => measurement.TreeId == id)
            .Where(measurement => measurement.EvidencePhotoUrl != null)
            .OrderByDescending(measurement => measurement.DeviceCapturedAt)
            .Select(measurement => measurement.EvidencePhotoUrl)
            .FirstOrDefaultAsync(cancellationToken);

        var assignedStudents = await context.UserCaresForTrees
            .AsNoTracking()
            .Where(assignment => assignment.TreeId == id)
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

        var locationDto = tree.Location is not null
            ? new LocationDto(
                tree.Location.Latitude,
                tree.Location.Longitude,
                tree.Location.Street,
                tree.Location.Neighborhood,
                tree.Location.StreetNumber)
            : null;

        var response = new Response(
            Id: tree.Id,
            CurrentCampaignFolio: currentFolio,
            HealthState: tree.HealthState.ToString(),
            PlantingDate: tree.PlantingDate,
            Observations: tree.Notes,
            Value: tree.Value!.ValueName,
            Species: tree.Species!.CommonName,
            SpeciesScientificName: tree.Species!.ScientificName,
            SpeciesImageUrl: tree.Species?.ImageUrl,
            CurrentHeightCentimeters: tree.HeightCentimeters,
            CurrentDiameterCentimeters: tree.DiameterCentimeters,
            LatestPhotoUrl: latestPhoto,
            Location: locationDto,
            CurrentAssignedStudents: assignedStudents);

        return response;
    }
}
