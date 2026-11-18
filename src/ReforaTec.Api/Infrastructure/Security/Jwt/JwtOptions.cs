using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Infrastructure.Security.Jwt;

/// <summary>
/// Binds JWT secret key, issuer, audience, expiration, and session limit settings from configuration.
/// </summary>
internal sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public List<string> ValidAudiences { get; set; } = [];
    public string SecretKey { get; set; } = string.Empty;
    public int AccessTokenExpirationHours { get; set; } = 3;
    public int RefreshTokenExpirationDays { get; set; } = 21;

    /// <summary>
    /// Maximum number of simultaneous active sessions allowed per user role.
    /// Keys must match the <see cref="ReforaTec.Api.Entities.Enums.UserRole"/> enum names.
    /// </summary>
    public Dictionary<string, int> MaxSessionsPerRole { get; set; } = new();

    // Fallback >= 1 prevents EnforceMaxSessionsAsync from blocking all logins if a role is omitted in config.
    public int GetMaxSessionsForRole(UserRole role) =>
        MaxSessionsPerRole.GetValueOrDefault(role.ToString(), 2);
}
