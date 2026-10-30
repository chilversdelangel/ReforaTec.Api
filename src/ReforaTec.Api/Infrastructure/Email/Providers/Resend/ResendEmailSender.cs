using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using ErrorOr;
using Microsoft.Extensions.Options;

namespace ReforaTec.Api.Infrastructure.Email.Providers.Resend;

internal sealed partial class ResendEmailSender(
    HttpClient httpClient,
    IOptions<ResendOptions> resendOptions,
    IOptions<EmailOptions> emailOptions,
    ILogger<ResendEmailSender> logger) : IEmailSender
{
    private readonly ResendOptions _resendOptions = resendOptions.Value;
    private readonly EmailOptions _emailOptions = emailOptions.Value;

    public async Task<ErrorOr<Success>> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var payload = new ResendPayload(
            From: _emailOptions.From,
            To: message.To,
            Subject: message.Subject,
            Text: message.TextBody,
            Html: message.HtmlBody,
            ReplyTo: message.ReplyTo ?? _emailOptions.ReplyToEmail);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/emails");
        
        request.Content = JsonContent.Create(payload);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _resendOptions.ApiKey);

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return Result.Success;
            }

            var rawError = await response.Content.ReadAsStringAsync(cancellationToken);
            var sanitizedError = rawError.Length > 500
                ? string.Concat(rawError.AsSpan(0, 500), "... [truncated]")
                : rawError;

            LogProviderError(logger, (int)response.StatusCode, message.To, sanitizedError);

            return Error.Failure();
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            LogUnexpectedError(logger, exception, message.To);
            return Error.Unexpected();
        }
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Resend API returned status code {StatusCode} when sending email to {Recipient}: {Error}")]
    private static partial void LogProviderError(ILogger logger, int statusCode, string recipient, string error);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Unexpected error communicating with Resend API for {Recipient}.")]
    private static partial void LogUnexpectedError(ILogger logger, Exception exception, string recipient);

    private sealed record ResendPayload(
        [property: JsonPropertyName("from")] string From,
        [property: JsonPropertyName("to")] string To,
        [property: JsonPropertyName("subject")] string Subject,
        [property: JsonPropertyName("text")] string Text,
        [property: JsonPropertyName("html")][property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Html,
        [property: JsonPropertyName("reply_to")][property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? ReplyTo);
}
