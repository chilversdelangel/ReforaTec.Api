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
            .Include(t => t.Species)
            .Include(t => t.Value)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (tree is null)
        {
            return Error.NotFound(
                code: ErrorCodes.NotFound,
                description: $"Tree with ID {id} was not found.");
        }

        var currentFolio = await context.CampaignManagesTrees
            .AsNoTracking()
            .Where(c => c.TreeId == id && c.EndDate == null)
            .Select(c => c.CampaignFolio)
            .FirstOrDefaultAsync(cancellationToken);

        var latestPhoto = await context.Measurements
            .AsNoTracking()
            .Where(m => m.TreeId == id && m.EvidencePhotoUrl != null)
            .OrderByDescending(m => m.DeviceCapturedAt)
            .Select(m => m.EvidencePhotoUrl)
            .FirstOrDefaultAsync(cancellationToken);

        var assignedStudents = await context.UserCaresForTrees
            .AsNoTracking()
            .Where(u => u.TreeId == id)
            .Where(u => u.EndDate == null)
            .Where(u => u.User != null && !u.User.IsDeleted)
            .Select(u => new AssignedStudentDto(
                u.User!.Id,
                u.User.ControlNumber,
                u.User.FirstName,
                u.User.MiddleName,
                u.User.LastName,
                u.User.SecondLastName))
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
