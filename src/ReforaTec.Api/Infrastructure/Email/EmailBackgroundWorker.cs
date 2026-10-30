using System.Threading.Channels;

namespace ReforaTec.Api.Infrastructure.Email;

internal sealed partial class EmailBackgroundWorker(
    Channel<EmailMessage> channel,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<EmailBackgroundWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var message in channel.Reader.ReadAllAsync(stoppingToken))
            {
                await ProcessMessageAsync(message, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            LogWorkerStopping(logger);
        }
        catch (Exception exception)
        {
            LogWorkerUnhandledError(logger, exception);
        }
    }

    private async Task ProcessMessageAsync(EmailMessage message, CancellationToken stoppingToken)
    {
        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

            var result = await emailSender.SendAsync(message, stoppingToken);

            if (!result.IsError)
            {
                LogDeliverySuccess(logger, message.To);
            }
        }
        catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
        {
            LogMessageProcessingFailed(logger, exception, message.To);
        }
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Email delivered successfully to {Recipient}.")]
    private static partial void LogDeliverySuccess(ILogger logger, string recipient);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Email background worker is stopping due to host shutdown.")]
    private static partial void LogWorkerStopping(ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Critical,
        Message = "Fatal unhandled exception in email background worker.")]
    private static partial void LogWorkerUnhandledError(ILogger logger, Exception exception);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Failed to process email message for {Recipient}.")]
    private static partial void LogMessageProcessingFailed(ILogger logger, Exception exception, string recipient);
}
