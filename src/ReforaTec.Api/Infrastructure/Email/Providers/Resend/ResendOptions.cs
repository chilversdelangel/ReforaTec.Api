namespace ReforaTec.Api.Infrastructure.Email.Providers.Resend;

internal sealed class ResendOptions
{
    public const string SectionName = "Notifications:Email:Providers:Resend";

    public string ApiKey { get; init; } = string.Empty;

    /// <summary>
    /// Base URL for the Resend REST API. Defaults to the official production endpoint,
    /// but configurable to allow mock servers or regional gateways.
    /// </summary>
    public string BaseUrl { get; init; } = "https://api.resend.com";

    /// <summary>
    /// User-Agent header value sent with requests to the Resend REST API.
    /// </summary>
    public string UserAgent { get; init; } = "reforatec-app";

    /// <summary>
    /// HTTP request timeout in seconds for Resend REST API calls. Defaults to 15 seconds.
    /// </summary>
    public int TimeoutSeconds { get; init; } = 15;
}
