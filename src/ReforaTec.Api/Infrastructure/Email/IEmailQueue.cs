namespace ReforaTec.Api.Infrastructure.Email;

internal interface IEmailQueue
{
    public ValueTask EnqueueAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
