using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using ReforaTec.Api.Infrastructure.Ingress;

namespace ReforaTec.Api.Infrastructure.Security.RateLimiting;

internal static class RateLimitingExtensions
{
    public static IServiceCollection AddCustomRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(RateLimitingOptions.SectionName);
        var options = section.Get<RateLimitingOptions>() ?? new RateLimitingOptions();

        services.Configure<RateLimitingOptions>(section);

        services.AddRateLimiter(limiterOptions =>
        {
            limiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            limiterOptions.OnRejected = HandleRejectedRequestAsync;

            limiterOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                CreateIpFixedWindowPolicy(httpContext, options.GlobalMaxRequests, options.GlobalWindowMinutes));

            limiterOptions.AddPolicy(RateLimitingPolicies.Otp, httpContext =>
                CreateIpFixedWindowPolicy(httpContext, options.OtpIpMaxRequests, options.OtpIpWindowMinutes));
        });

        return services;
    }

    private static RateLimitPartition<string> CreateIpFixedWindowPolicy(
        HttpContext httpContext,
        int maxRequests,
        int windowMinutes)
    {
        var clientIp = httpContext.RequestServices.GetRequiredService<IClientIpResolver>().Resolve(httpContext);

        return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = maxRequests,
            Window = TimeSpan.FromMinutes(windowMinutes),
            QueueLimit = 0
        });
    }

    private static async ValueTask HandleRejectedRequestAsync(
        OnRejectedContext context,
        CancellationToken cancellationToken)
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
        }

        await RateLimitingErrors.TooManyRequests().ExecuteAsync(context.HttpContext);
    }
}
