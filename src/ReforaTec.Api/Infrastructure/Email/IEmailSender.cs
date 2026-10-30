using ErrorOr;

namespace ReforaTec.Api.Infrastructure.Email;

internal interface IEmailSender
{
    public Task<ErrorOr<Success>> SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
