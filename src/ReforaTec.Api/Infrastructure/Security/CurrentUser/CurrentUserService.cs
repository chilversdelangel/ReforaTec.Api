using System.Security.Claims;
using ErrorOr;

namespace ReforaTec.Api.Infrastructure.Security.CurrentUser;

internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private static readonly Error UnauthorizedError = Error.Unauthorized(
        code: "auth/unauthorized",
        description: "User identity could not be determined from the authentication token.");

    private readonly ClaimsPrincipal? _user = httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => _user?.Identity?.IsAuthenticated ?? false;

    public int? UserId => GetClaimAsInt(ClaimTypes.NameIdentifier);

    public int? TenantId => GetClaimAsInt(CustomClaims.TenantId);

    public ErrorOr<UserIdentity> GetIdentity()
    {
        if (!IsAuthenticated || UserId is null || TenantId is null)
        {
            return UnauthorizedError;
        }

        return new UserIdentity(UserId.Value, TenantId.Value);
    }

    private int? GetClaimAsInt(string claimType)
    {
        var claimValue = _user?.FindFirstValue(claimType);
        
        if (int.TryParse(claimValue, out var value))
        {
            return value;
        }

        return null;
    }
}
