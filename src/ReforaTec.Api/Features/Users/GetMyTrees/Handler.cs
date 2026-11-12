using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
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

        var assignedCareRecords =
            await GetActiveAssignmentsAsync(context, tenantId, userId, cancellationToken);

        if (assignedCareRecords.Count == 0)
        {
            return new Response([]);
        }

        var treeIds = assignedCareRecords.Select(assignment => assignment.TreeId).ToList();

        var latestPhotoByTreeId =
            await GetLatestPhotosAsync(context, tenantId, treeIds, cancellationToken);

        var assignedTrees = assignedCareRecords
            .Select(assignment => MapToAssignedTreeDto(assignment, latestPhotoByTreeId))
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

    private static AssignedTreeDto MapToAssignedTreeDto(
        CareAssignment assignment,
        IReadOnlyDictionary<int, string?> latestPhotoByTreeId)
    {
        var tree = assignment.Tree!;
        var latestPhotoUrl = latestPhotoByTreeId.GetValueOrDefault(tree.Id);

        return new AssignedTreeDto(
            Id: tree.Id,
            Folio: tree.Folio,
            
            SpeciesId: tree.SpeciesId,
            CommonName: tree.Species!.CommonName,
            ScientificName: tree.Species.ScientificName,
            SpeciesImageUrl: tree.Species.ImageUrl,
            
            VirtueName: tree.Virtue!.Name,
            CurrentCampaignName: assignment.Campaign?.CampaignName,
            
            HealthState: tree.HealthState.ToString(),
            PlantingDate: tree.PlantingDate,
            
            CurrentHeightCentimeters: tree.HeightCentimeters,
            CurrentDiameterCentimeters: tree.DiameterCentimeters,
            
            LatestPhotoUrl: latestPhotoUrl);
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
            .Include(assignment => assignment.Campaign)
            .Include(assignment => assignment.Tree)
            .ThenInclude(tree => tree!.Species)
            .Include(assignment => assignment.Tree)
            .ThenInclude(tree => tree!.Virtue)
            .OrderBy(assignment => assignment.TreeId)
            .ToListAsync(cancellationToken);
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
            .Where(measurement => measurement.EvidencePhotoUrl != null)
            .GroupBy(measurement => measurement.TreeId)
            .Select(treeGroup => new LatestPhotoResult(
                treeGroup.Key,
                treeGroup.OrderByDescending(measurement => measurement.DeviceCapturedAt)
                    .Select(measurement => measurement.EvidencePhotoUrl)
                    .FirstOrDefault()
            ))
            .ToDictionaryAsync(
                result => result.TreeId, 
                result => result.LatestPhotoUrl, 
                cancellationToken);
    }

    private sealed record LatestPhotoResult(int TreeId, string? LatestPhotoUrl);
}