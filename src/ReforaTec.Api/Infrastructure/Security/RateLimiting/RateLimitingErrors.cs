using ReforaTec.Api.Infrastructure.Mapping;

namespace ReforaTec.Api.Infrastructure.Security.RateLimiting;

internal static class RateLimitingErrors
{
    public static IResult TooManyRequests() =>
        Results.Problem(
            statusCode: StatusCodes.Status429TooManyRequests,
            title: "Too Many Requests",
            type: ErrorTypes.TooManyRequests,
            detail: "Rate limit exceeded. Please try again later.");
}
