using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.Users.UpdateUserProfile;

internal static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        Request request,
        UserIdentity loggedUser,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var userEntity = await FindUserAsync(loggedUser, context, cancellationToken);
        if (userEntity is null)
        {
            return Errors.UserNotFound(loggedUser.UserId);
        }

        ApplyUpdates(userEntity, request);

        userEntity.Normalize();

        await context.SaveChangesAsync(cancellationToken);

        return new Response(
            Id: userEntity.Id,
            GivenName: userEntity.GivenName,
            FamilyName: userEntity.FamilyName,
            SecondFamilyName: userEntity.SecondFamilyName,
            DisplayName: userEntity.DisplayName,
            ControlNumber: userEntity.ControlNumber,
            PhoneNumber: userEntity.PhoneNumber,
            Gender: userEntity.Gender,
            Email: userEntity.Email,
            CurrentRole: userEntity.CurrentRole);
    }

    private static void ApplyUpdates(User userEntity, Request request)
    {
        if (request.GivenName is not null)
        {
            userEntity.GivenName = request.GivenName;
        }

        if (request.FamilyName is not null)
        {
            userEntity.FamilyName = request.FamilyName;
        }

        if (request.SecondFamilyName is not null)
        {
            userEntity.SecondFamilyName = request.SecondFamilyName.ToSanitizedOrNull();
        }

        if (request.DisplayName is not null)
        {
            userEntity.DisplayName = request.DisplayName.ToSanitizedOrNull();
        }

        if (request.PhoneNumber is not null)
        {
            userEntity.PhoneNumber = request.PhoneNumber;
        }

        if (request.Gender.HasValue)
        {
            userEntity.Gender = request.Gender.Value;
        }
    }

    private static async Task<User?> FindUserAsync(
        UserIdentity loggedUser,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        return await context.Users
            .Where(userEntity => userEntity.TenantId == loggedUser.TenantId)
            .Where(userEntity => userEntity.Id == loggedUser.UserId)
            .Where(userEntity => !userEntity.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }
}