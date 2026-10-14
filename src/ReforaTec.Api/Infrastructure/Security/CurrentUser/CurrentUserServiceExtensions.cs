namespace ReforaTec.Api.Infrastructure.Security.CurrentUser;

internal static class CurrentUserServiceExtensions
{
    public static IServiceCollection AddCurrentUserService(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
