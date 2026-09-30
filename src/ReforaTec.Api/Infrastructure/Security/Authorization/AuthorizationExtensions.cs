using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Infrastructure.Security.Authorization;

internal static class AuthorizationExtensions
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddAuthorizationPolicies()
        {
            var auth = services.AddAuthorizationBuilder();

            auth.AddPolicy(Policy.CanManageCatalogs, policy => policy.RequireRole(
                nameof(UserRole.SystemAdmin)));

            return services;
        }
    }
}
