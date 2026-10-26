namespace ReforaTec.Api.Infrastructure.Security.RateLimiting;

internal sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public int OtpIpMaxRequests { get; init; } = 30;
    public int OtpIpWindowMinutes { get; init; } = 1;

    public int OtpEmailMaxAttempts { get; init; } = 3;
    public int OtpEmailWindowMinutes { get; init; } = 5;

    public int GlobalMaxRequests { get; init; } = 100;
    public int GlobalWindowMinutes { get; init; } = 1;
}
