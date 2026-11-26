using ErrorOr;

namespace ReforaTec.Api.Features.Users.GetUsers;

internal static class Errors
{
    public static readonly Error Forbidden = Error.Forbidden(
        code: "auth/forbidden",
        description: "You do not have permission to list users.");
}
