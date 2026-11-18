using ErrorOr;
using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Infrastructure.Security.CurrentUser;

internal sealed record UserIdentity(int UserId, int TenantId, UserRole Role);

internal interface ICurrentUserService
{
    public int? UserId { get; }
    public int? TenantId { get; }
    public UserRole? Role { get; }
    public bool IsAuthenticated { get; }

    public ErrorOr<UserIdentity> GetIdentity();
}
