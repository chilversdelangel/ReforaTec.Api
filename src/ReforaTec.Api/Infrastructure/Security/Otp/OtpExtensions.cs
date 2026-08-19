namespace ReforaTec.Api.Infrastructure.Security.Otp;

internal static class OtpExtensions
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddOtpService<TOtpService>()
            where TOtpService : class, IOtpService
        {
            services.AddScoped<IOtpService, TOtpService>();

            return services;
        }
    }
}
