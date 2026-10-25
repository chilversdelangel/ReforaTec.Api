namespace ReforaTec.Api.Infrastructure.Ingress;

internal interface IClientIpResolver
{
    string Resolve(HttpContext httpContext);
}
