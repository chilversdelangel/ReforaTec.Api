using System.Threading.Channels;
using ReforaTec.Api.Infrastructure.Email.Providers.Null;
using ReforaTec.Api.Infrastructure.Email.Providers.Resend;

namespace ReforaTec.Api.Infrastructure.Email;

internal static class EmailExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddEmailServices(IConfiguration configuration)
        {
            const string sectionName = EmailOptions.SectionName;
            
            var emailSection = configuration.GetSection(sectionName);
            var emailOptions = emailSection.Get<EmailOptions>() ?? new EmailOptions();

            services.AddOptions<EmailOptions>()
                .Bind(emailSection)
                .ValidateOnStart();

            var channelOptions = new BoundedChannelOptions(1000)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            };

            services.AddSingleton(Channel.CreateBounded<EmailMessage>(channelOptions));
            services.AddSingleton<IEmailQueue, ChannelEmailQueue>();
            services.AddHostedService<EmailBackgroundWorker>();

            return emailOptions.Provider switch
            {
                EmailProvider.Resend => services.AddResendProvider(configuration),
                EmailProvider.None => services.AddScoped<IEmailSender, NullEmailSender>(),
                _ => throw new InvalidOperationException(
                    $"Unsupported email provider: '{emailOptions.Provider}'. Supported providers are 'Resend' and 'None'.")
            };
        }

        private IServiceCollection AddResendProvider(IConfiguration configuration)
        {
            const string sectionName = ResendOptions.SectionName;
            
            var resendSection = configuration.GetSection(sectionName);
            var resendOptions = resendSection.Get<ResendOptions>() ?? new ResendOptions();
            
            services.AddOptions<ResendOptions>()
                .Bind(resendSection)
                .Validate(
                    options => !string.IsNullOrWhiteSpace(options.ApiKey),
                    "Resend API key is required when email provider is configured to 'Resend'.")
                .ValidateOnStart();

            services.AddHttpClient<IEmailSender, ResendEmailSender>(client =>
                ConfigureResendClient(client, resendOptions));

            return services;
        }
    }

    private static void ConfigureResendClient(HttpClient client, ResendOptions options)
    {
        client.BaseAddress = new Uri(options.BaseUrl);
        client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
    }
}
