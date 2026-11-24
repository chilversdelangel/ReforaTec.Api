using ErrorOr;

namespace ReforaTec.Api.Features.Users.UpdateUser;

internal static class Errors
{
    public static readonly Error Forbidden = Error.Forbidden(
        code: "auth/forbidden",
        description: "You do not have permission to update this user.");

    public static Error UserNotFound(int userId) => Error.NotFound(
        code: "user/not-found",
        description: $"User with ID {userId} was not found.");
}
