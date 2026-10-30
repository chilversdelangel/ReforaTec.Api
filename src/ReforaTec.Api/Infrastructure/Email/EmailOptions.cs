namespace ReforaTec.Api.Infrastructure.Email;

internal sealed class EmailOptions
{
    public const string SectionName = "Notifications:Email";

    public string SenderName { get; init; } = "ReforaTec";

    public string SenderEmail { get; init; } = "notificaciones@auth.reforatec.org";

    public string? ReplyToEmail { get; init; } = "soporte@reforatec.org";

    public EmailProvider Provider { get; init; } = EmailProvider.Resend;

    public string From => $"{SenderName} <{SenderEmail}>";
}
