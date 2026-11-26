using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.Users.GetUsers;

internal static class Handler
{
    public static async Task<ErrorOr<IReadOnlyList<Response>>> Handle(
        UserIdentity loggedUser,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        if (loggedUser.Role is not (UserRole.Coordinator or UserRole.SystemAdmin))
        {
            return Errors.Forbidden;
        }

        var users = await GetSubordinateUsersAsync(context, loggedUser, cancellationToken);
        return ErrorOrFactory.From(users);
    }

    private static async Task<IReadOnlyList<Response>> GetSubordinateUsersAsync(
        AppDbContext context,
        UserIdentity loggedUser,
        CancellationToken cancellationToken)
    {
        return await context.Users
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(user => user.TenantId == loggedUser.TenantId)
            .Where(user => user.Role < loggedUser.Role)
            .OrderBy(user => user.Id)
            .Select(user => new Response(
                user.Id,
                user.GivenName,
                user.FamilyName,
                user.SecondFamilyName,
                user.DisplayName,

                user.ControlNumber,
                user.PhoneNumber,
                user.Gender,
                user.Email,
                user.IsEmailVerified,
                user.Role,

                user.IsDeleted,
                user.DeletedAt,
                user.CreatedAt,
                user.ModifiedAt))
            .ToListAsync(cancellationToken);
    }
}
