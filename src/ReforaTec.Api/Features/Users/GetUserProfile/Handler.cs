using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.Users.GetUserProfile;

internal static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        UserIdentity loggedUser,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var userEntity = await FindUserAsync(context, loggedUser, cancellationToken);
        if (userEntity is null)
        {
            return Errors.UserNotFound(loggedUser.UserId);
        }

        var totalTrees = await GetAssignedTreesCountAsync(context, loggedUser, cancellationToken);
        var totalServices = await GetServicesCountAsync(context, loggedUser, cancellationToken);
        var currentCampaign = await GetCurrentCampaignNameAsync(context, loggedUser, cancellationToken);

        return new Response(
            FirstName: userEntity.FirstName,
            LastName: userEntity.LastName,
            SecondLastName: userEntity.SecondLastName,
            DisplayName: userEntity.DisplayName,
            ControlNumber: userEntity.ControlNumber,
            PhoneNumber: userEntity.PhoneNumber,
            Gender: userEntity.Gender,
            Email: userEntity.Email,
            CurrentCampaign: currentCampaign,
            TotalTrees: totalTrees,
            TotalServices: totalServices);
    }

    private static Task<User?> FindUserAsync(
        AppDbContext context,
        UserIdentity loggedUser,
        CancellationToken cancellationToken)
    {
        return context.Users
            .AsNoTracking()
            .Where(user => user.TenantId == loggedUser.TenantId)
            .Where(user => user.Id == loggedUser.UserId)
            .Where(user => !user.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static Task<int> GetAssignedTreesCountAsync(
        AppDbContext context,
        UserIdentity loggedUser,
        CancellationToken cancellationToken)
    {
        return context.UserCaresForTrees
            .AsNoTracking()
            .Where(assignment => assignment.TenantId == loggedUser.TenantId)
            .Where(assignment => assignment.UserId == loggedUser.UserId)
            .Where(assignment => assignment.EndDate == null)
            .CountAsync(cancellationToken);
    }

    private static Task<int> GetServicesCountAsync(
        AppDbContext context,
        UserIdentity loggedUser,
        CancellationToken cancellationToken)
    {
        return context.Services
            .AsNoTracking()
            .Where(service => service.TenantId == loggedUser.TenantId)
            .Where(service => service.StudentId == loggedUser.UserId)
            .CountAsync(cancellationToken);
    }

    private static Task<string?> GetCurrentCampaignNameAsync(
        AppDbContext context,
        UserIdentity loggedUser,
        CancellationToken cancellationToken)
    {
        var activeTreeIds = context.UserCaresForTrees
            .Where(assignment => assignment.TenantId == loggedUser.TenantId)
            .Where(assignment => assignment.UserId == loggedUser.UserId)
            .Where(assignment => assignment.EndDate == null)
            .Select(assignment => assignment.TreeId);

        return context.CampaignManagesTrees
            .AsNoTracking()
            .Where(campaignTree => campaignTree.TenantId == loggedUser.TenantId)
            .Where(campaignTree => campaignTree.EndDate == null)
            .Where(campaignTree => activeTreeIds.Contains(campaignTree.TreeId))
            .Select(campaignTree => campaignTree.Campaign!.CampaignName)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
