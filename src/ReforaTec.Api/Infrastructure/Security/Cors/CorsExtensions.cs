namespace ReforaTec.Api.Infrastructure.Security.Cors;

internal static class CorsExtensions
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddCorsPolicy(IConfiguration configuration)
        {
            var corsSection = configuration.GetSection(CorsPolicyOptions.SectionName);
            var corsOptions = corsSection.Get<CorsPolicyOptions>() ?? new CorsPolicyOptions();

            services.Configure<CorsPolicyOptions>(corsSection);

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    var allowedOrigins = corsOptions.AllowedOrigins;

                    if (allowedOrigins.Length == 0 || allowedOrigins.Contains("*"))
                    {
                        policy.AllowAnyOrigin();
                    }
                    else
                    {
                        policy.WithOrigins(allowedOrigins);
                    }

                    policy.AllowAnyMethod()
                        .AllowAnyHeader()
                        .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
                });
            });

            return services;
        }
    }
}