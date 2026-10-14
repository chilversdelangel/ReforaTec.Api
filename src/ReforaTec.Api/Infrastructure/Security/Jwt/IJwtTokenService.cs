using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Infrastructure.Security.Jwt;

internal sealed record TokenGenerationRequest(int UserId, int TenantId, UserRole Role, string Audience);

internal interface IJwtTokenService
{
    /// <summary>
    /// Generates a signed JWT Access Token for the specified user.
    /// </summary>
    string GenerateAccessToken(TokenGenerationRequest request);

    /// <summary>
    /// Generates a cryptographically secure raw Refresh Token string (64 random bytes in Base64).
    /// </summary>
    string GenerateRawRefreshToken();

    /// <summary>
    /// Hashes a raw Refresh Token string using SHA-256 (HEX output).
    /// </summary>
    string HashRefreshToken(string rawRefreshToken);
}
