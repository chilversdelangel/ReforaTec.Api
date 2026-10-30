using ErrorOr;

namespace ReforaTec.Api.Infrastructure.Email.Providers.Null;

internal sealed class NullEmailSender : IEmailSender
{
    public Task<ErrorOr<Success>> SendAsync(EmailMessage message, CancellationToken cancellationToken = default) =>
        Task.FromResult<ErrorOr<Success>>(Result.Success);
}
