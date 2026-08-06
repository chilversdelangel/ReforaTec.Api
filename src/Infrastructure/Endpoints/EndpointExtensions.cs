using System.Reflection;

namespace ReforaTec.Api.Infrastructure.Endpoints;

internal static class EndpointExtensions
{
    internal static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        var endpointTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IEndpoint).IsAssignableFrom(t)
                        && !t.IsAbstract
                        && !t.IsInterface);

        foreach (var type in endpointTypes)
        {
            var endpoint = Activator.CreateInstance(type) as IEndpoint
                           ?? throw new InvalidOperationException($"Could not create endpoint {type.FullName}");

            endpoint.MapEndpoint(app);
        }

        return app;
    }
}