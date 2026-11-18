using ErrorOr;

namespace ReforaTec.Api.Features.Auth.RefreshSession;

internal static class Errors
{
    public static readonly Error InvalidToken = Error.Unauthorized(
        code: "auth/invalid-refresh-token",
        description: "Invalid refresh token.");

    public static readonly Error TokenExpired = Error.Unauthorized(
        code: "auth/refresh-token-expired",
        description: "Refresh token has expired. Please log in again.");
}
