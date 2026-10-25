namespace ReforaTec.Api.Infrastructure.Ingress;

internal static class IngressExtensions
{
    public static IServiceCollection AddIngress(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(IngressOptions.SectionName);

        services.Configure<IngressOptions>(section);
        services.AddSingleton<IClientIpResolver, ClientIpResolver>();

        return services;
    }
}
