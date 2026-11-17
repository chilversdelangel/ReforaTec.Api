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
        var totalServices = await GetCareLogsCountAsync(context, loggedUser, cancellationToken);
        var isEnrolledInActiveCampaign = await IsEnrolledInActiveCampaignAsync(context, loggedUser, cancellationToken);

        return new Response(
            GivenName: userEntity.GivenName,
            FamilyName: userEntity.FamilyName,
            SecondFamilyName: userEntity.SecondFamilyName,
            DisplayName: userEntity.DisplayName,

            ControlNumber: userEntity.ControlNumber,
            PhoneNumber: userEntity.PhoneNumber,
            Gender: userEntity.Gender,
            Email: userEntity.Email,

            IsEnrolledInActiveCampaign: isEnrolledInActiveCampaign,
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
        return context.CareAssignments
            .AsNoTracking()
            .Where(assignment => assignment.TenantId == loggedUser.TenantId)
            .Where(assignment => assignment.UserId == loggedUser.UserId)
            .Where(assignment => assignment.EndDate == null)
            .CountAsync(cancellationToken);
    }

    private static Task<int> GetCareLogsCountAsync(
        AppDbContext context,
        UserIdentity loggedUser,
        CancellationToken cancellationToken)
    {
        return context.CareLogs
            .AsNoTracking()
            .Where(careLog => careLog.TenantId == loggedUser.TenantId)
            .Where(careLog => careLog.UserId == loggedUser.UserId)
            .CountAsync(cancellationToken);
    }

    private static Task<bool> IsEnrolledInActiveCampaignAsync(
        AppDbContext context,
        UserIdentity loggedUser,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return context.CampaignEnrollments
            .AsNoTracking()
            .Where(enrollment => enrollment.TenantId == loggedUser.TenantId)
            .Where(enrollment => enrollment.UserId == loggedUser.UserId)
            .Where(enrollment => enrollment.IsApproved == null)
            .Where(enrollment =>
                enrollment.Campaign!.Period.EndDate == null ||
                enrollment.Campaign.Period.EndDate >= today)
            .AnyAsync(cancellationToken);
    }
}
