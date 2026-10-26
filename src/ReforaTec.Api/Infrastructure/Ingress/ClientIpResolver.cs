using Microsoft.Extensions.Options;

namespace ReforaTec.Api.Infrastructure.Ingress;

internal sealed class ClientIpResolver(IOptions<IngressOptions> options) : IClientIpResolver
{
    private const string FallbackAnonymousIp = "anonymous";
    private readonly IngressOptions _options = options.Value;

    private string? RealIpHeader => _options.RealIpHeader;

    public string Resolve(HttpContext httpContext)
    {
        // Priority chain for ingress & reverse proxies:
        // 1. Explicit header from appsettings.json (e.g. AWS/Custom CDN)
        // 2. Cloudflare edge IP
        // 3. Nginx / HAProxy standard real IP
        // 4. Standard proxy chain (first client IP in comma-separated list)
        // 5. Direct TCP socket connection (local dev / testcontainers)
        // 6. Safe fallback for headless/mock environments
        return FromHeader(RealIpHeader)
            ?? FromHeader("CF-Connecting-IP")
            ?? FromHeader("X-Real-IP")
            ?? FromHeader("X-Forwarded-For")
            ?? httpContext.Connection.RemoteIpAddress?.ToString()
            ?? FallbackAnonymousIp;

        string? FromHeader(string? headerName) => GetHeaderIp(httpContext, headerName);
    }

    private static string? GetHeaderIp(HttpContext httpContext, string? headerName)
    {
        if (string.IsNullOrWhiteSpace(headerName))
        {
            return null;
        }

        if (!httpContext.Request.Headers.TryGetValue(headerName, out var values))
        {
            return null;
        }

        var ip = values.ToString().Split(',')[0].Trim();
        
        return string.IsNullOrWhiteSpace(ip) ? null : ip;
    }
}
