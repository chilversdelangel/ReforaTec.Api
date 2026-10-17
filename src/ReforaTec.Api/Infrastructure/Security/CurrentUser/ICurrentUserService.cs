using ErrorOr;

namespace ReforaTec.Api.Infrastructure.Security.CurrentUser;

internal sealed record UserIdentity(int UserId, int TenantId);

internal interface ICurrentUserService
{
    public int? UserId { get; }
    public int? TenantId { get; }
    public bool IsAuthenticated { get; }

    public ErrorOr<UserIdentity> GetIdentity();
}
