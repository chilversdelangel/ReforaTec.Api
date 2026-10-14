using ErrorOr;

namespace ReforaTec.Api.Infrastructure.Security.CurrentUser;

internal sealed record UserIdentity(int UserId, int TenantId);

internal interface ICurrentUserService
{
    int? UserId { get; }
    int? TenantId { get; }
    bool IsAuthenticated { get; }

    ErrorOr<UserIdentity> GetIdentity();
}
