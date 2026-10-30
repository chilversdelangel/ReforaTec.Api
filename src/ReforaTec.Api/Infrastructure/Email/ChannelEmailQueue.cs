using System.Threading.Channels;

namespace ReforaTec.Api.Infrastructure.Email;

internal sealed class ChannelEmailQueue(Channel<EmailMessage> channel) : IEmailQueue
{
    public ValueTask EnqueueAsync(EmailMessage message, CancellationToken cancellationToken = default) =>
        channel.Writer.WriteAsync(message, cancellationToken);
}
