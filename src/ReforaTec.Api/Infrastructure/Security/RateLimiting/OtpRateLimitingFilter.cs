using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Features.Auth.RequestOtp;

namespace ReforaTec.Api.Infrastructure.Security.RateLimiting;

internal sealed record RateLimitState(int AttemptCount, DateTimeOffset ResetsAt)
{
    public bool HasExceeded(int limit) => AttemptCount > limit;

    public bool HasReset(DateTimeOffset attemptTime) => ResetsAt <= attemptTime;

    public int GetSecondsUntilReset(DateTimeOffset attemptTime) =>
        Math.Max(1, (int)Math.Ceiling((ResetsAt - attemptTime).TotalSeconds));
}

internal sealed class OtpRateLimitingFilter(
    IMemoryCache cache,
    IOptions<RateLimitingOptions> options) : IEndpointFilter
{
    private readonly RateLimitingOptions _options = options.Value;

    private int MaxAllowedAttempts => _options.OtpEmailMaxAttempts;
    private TimeSpan WindowDuration => TimeSpan.FromMinutes(_options.OtpEmailWindowMinutes);

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<Request>().FirstOrDefault();
        if (request is null || string.IsNullOrWhiteSpace(request.Email))
        {
            return await next(context);
        }

        var attemptTime = DateTimeOffset.UtcNow;

        var rateLimitState = RecordAttempt(request.Email, attemptTime);
        if (rateLimitState.HasExceeded(MaxAllowedAttempts))
        {
            context.HttpContext.Response.Headers.RetryAfter
                = rateLimitState.GetSecondsUntilReset(attemptTime).ToString();

            return RateLimitingErrors.TooManyRequests();
        }

        return await next(context);
    }

    private RateLimitState RecordAttempt(string email, DateTimeOffset attemptTime)
    {
        var cacheKey = $"otp_rate_{email.ToNormalized()}";
        var currentState = cache.Get<RateLimitState>(cacheKey);

        var isNewWindow = currentState is null || currentState.HasReset(attemptTime);
        var updatedState = isNewWindow
            ? new RateLimitState(1, attemptTime.Add(WindowDuration))
            : currentState! with { AttemptCount = currentState.AttemptCount + 1 };

        cache.Set(cacheKey, updatedState, updatedState.ResetsAt);

        return updatedState;
    }
}