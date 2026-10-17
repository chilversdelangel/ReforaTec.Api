using ErrorOr;

namespace ReforaTec.Api.Features.Users.UpdateUserProfile;

internal static class Errors
{
    public static Error UserNotFound(int userId) =>
        Error.NotFound(
            code: "user/not-found",
            description: $"User with ID {userId} was not found.");
}
