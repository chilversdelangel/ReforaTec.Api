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

        return new Response(
            Id: userEntity.Id,
            GivenName: userEntity.GivenName,
            FamilyName: userEntity.FamilyName,
            SecondFamilyName: userEntity.SecondFamilyName,
            DisplayName: userEntity.DisplayName,

            ControlNumber: userEntity.ControlNumber,
            PhoneNumber: userEntity.PhoneNumber,
            Gender: userEntity.Gender.ToString(),
            Email: userEntity.Email,
            Role: userEntity.Role.ToString());
    }

    private static Task<User?> FindUserAsync(
        AppDbContext context,
        UserIdentity loggedUser,
        CancellationToken cancellationToken) =>
        context.Users
            .AsNoTracking()
            .Where(user => user.TenantId == loggedUser.TenantId)
            .Where(user => user.Id == loggedUser.UserId)
            .FirstOrDefaultAsync(cancellationToken);
}
