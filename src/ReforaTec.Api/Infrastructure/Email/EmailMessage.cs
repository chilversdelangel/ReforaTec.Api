namespace ReforaTec.Api.Infrastructure.Email;

internal sealed record EmailMessage(
    string To,
    string Subject,
    string TextBody,
    string? HtmlBody = null,
    string? ReplyTo = null);
