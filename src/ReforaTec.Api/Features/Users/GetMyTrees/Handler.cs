using System.Security.Claims;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Security;

namespace ReforaTec.Api.Features.Users.GetMyTrees;

internal static class Handler
{
    public static async Task<ErrorOr<List<Response>>> Handle(
        ClaimsPrincipal user,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();

        if (userId is null)
        {
            return Error.Unauthorized(
                code: ErrorCodes.Unauthorized,
                description: "User identity could not be determined from the authentication token.");
        }

        var userExists = await UserExistsAsync(context, userId.Value, cancellationToken);

        if (!userExists)
        {
            return Error.NotFound(
                code: ErrorCodes.UserNotFound,
                description: $"User with ID {userId} was not found.");
        }

        var assignedCareRecords =
            await GetActiveAssignmentsAsync(context, userId.Value, cancellationToken);

        if (assignedCareRecords.Count == 0)
        {
            return new List<Response>();
        }

        var treeIds = assignedCareRecords.Select(assignment => assignment.TreeId).ToList();

        var activeCampaignByTreeId =
            await GetActiveCampaignsAsync(context, treeIds, cancellationToken);

        var latestPhotoByTreeId =
            await GetLatestPhotosAsync(context, treeIds, cancellationToken);

        var response = assignedCareRecords.Select(assignment =>
        {
            var tree = assignment.Tree!;
            
            var campaignRecord = activeCampaignByTreeId.GetValueOrDefault(tree.Id);
            var latestPhotoUrl = latestPhotoByTreeId.GetValueOrDefault(tree.Id);

            return new Response(
                Id: tree.Id,
                
                SpeciesId: tree.SpeciesId,
                CommonName: tree.Species!.CommonName,
                ScientificName: tree.Species!.ScientificName,
                SpeciesImageUrl: tree.Species!.ImageUrl,
                
                ValueName: tree.Value!.ValueName,
                
                CurrentCampaignName: campaignRecord?.Campaign?.CampaignName,
                CurrentCampaignFolio: campaignRecord?.CampaignFolio,
                
                HealthState: tree.HealthState.ToString(),
                PlantingDate: tree.PlantingDate,
                
                CurrentHeightCentimeters: tree.HeightCentimeters,
                CurrentDiameterCentimeters: tree.DiameterCentimeters,
                
                LatestPhotoUrl: latestPhotoUrl);
        }).ToList();

        return response;
    }

    private static async Task<List<Entities.UserCaresForTree>> GetActiveAssignmentsAsync(
        AppDbContext context,
        int userId,
        CancellationToken cancellationToken)
    {
        return await context.UserCaresForTrees
            .AsNoTracking()
            .Where(assignment => assignment.UserId == userId)
            .Where(assignment => assignment.EndDate == null)
            .Where(assignment => assignment.Tree != null)
            .Include(assignment => assignment.Tree)
            .ThenInclude(t => t!.Species)
            .Include(assignment => assignment.Tree)
            .ThenInclude(t => t!.Value)
            .OrderBy(assignment => assignment.TreeId)
            .ToListAsync(cancellationToken);
    }

    private static Task<bool> UserExistsAsync(AppDbContext context, int userId, CancellationToken cancellationToken)
    {
        return context.Users
            .Where(user => user.Id == userId)
            .Where(user => !user.IsDeleted)
            .AnyAsync(cancellationToken);
    }

    private static Task<Dictionary<int, Entities.CampaignManagesTree>> GetActiveCampaignsAsync(
        AppDbContext context,
        List<int> treeIds,
        CancellationToken cancellationToken)
    {
        return context.CampaignManagesTrees
            .AsNoTracking()
            .Where(campaignRecord => treeIds.Contains(campaignRecord.TreeId))
            .Where(campaignRecord => campaignRecord.EndDate == null)
            .Include(campaignRecord => campaignRecord.Campaign)
            .ToDictionaryAsync(campaignRecord => campaignRecord.TreeId, cancellationToken);
    }

    private static Task<Dictionary<int, string?>> GetLatestPhotosAsync(
        AppDbContext context,
        List<int> treeIds,
        CancellationToken cancellationToken)
    {
        return context.Measurements
            .AsNoTracking()
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