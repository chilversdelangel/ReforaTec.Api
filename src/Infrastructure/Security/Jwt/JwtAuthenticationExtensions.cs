using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace ReforaTec.Api.Infrastructure.Security.Jwt;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication<TTokenService>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TTokenService : class, IJwtTokenService
    {
        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        var jwtOptions = jwtSection.Get<JwtOptions>()!;

        services.Configure<JwtOptions>(jwtSection);
        services.AddSingleton<IJwtTokenService, TTokenService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => options.TokenValidationParameters = CreateTokenValidationParameters(jwtOptions));

        var fallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(fallbackPolicy);

        return services;
    }

    private static TokenValidationParameters CreateTokenValidationParameters(JwtOptions jwtOptions)
    {
        return new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudiences = jwtOptions.ValidAudiences,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }
}
