using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.Users.UpdateUser;

internal static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        UserIdentity loggedUser,
        int userId,
        Request request,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        if (loggedUser.Role is not (UserRole.Coordinator or UserRole.SystemAdmin))
        {
            return Errors.Forbidden;
        }

        if (request.Role.HasValue && request.Role.Value >= loggedUser.Role)
        {
            return Errors.Forbidden;
        }

        var targetUser = await FindTargetUserAsync(context, loggedUser.TenantId, userId, cancellationToken);
        if (targetUser is null)
        {
            return Errors.UserNotFound(userId);
        }

        if (targetUser.Role >= loggedUser.Role)
        {
            return Errors.Forbidden;
        }

        if (request.Role.HasValue)
        {
            targetUser.Role = request.Role.Value;
            await context.SaveChangesAsync(cancellationToken);
        }

        return MapToResponse(targetUser);
    }

    private static Task<User?> FindTargetUserAsync(
        AppDbContext context,
        int tenantId,
        int userId,
        CancellationToken cancellationToken)
    {
        return context.Users
            .Where(user => user.TenantId == tenantId)
            .Where(user => user.Id == userId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static Response MapToResponse(User user) => new(
        user.Id,

        user.GivenName,
        user.FamilyName,
        user.SecondFamilyName,
        user.DisplayName,

        user.ControlNumber,
        user.PhoneNumber,
        user.Gender.ToString(),

        user.Email,
        user.Role.ToString());
}
