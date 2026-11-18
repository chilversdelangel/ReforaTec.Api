namespace ReforaTec.Api.Infrastructure.Security.Otp;

internal static class OtpExtensions
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddOtpService<TOtpService>(IConfiguration configuration)
            where TOtpService : class, IOtpService
        {
            var otpSection = configuration.GetSection(OtpOptions.SectionName);

            services.Configure<OtpOptions>(otpSection);
            services.AddScoped<IOtpService, TOtpService>();

            return services;
        }
    }
}
