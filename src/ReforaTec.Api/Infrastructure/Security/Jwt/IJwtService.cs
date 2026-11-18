using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Infrastructure.Security.Jwt;

internal sealed record TokenGenerationRequest(int UserId, int TenantId, UserRole Role, string Audience);

internal sealed record GeneratedRefreshToken(string RawToken, string HashedToken, DateTime ExpiresAt);

internal interface IJwtService
{
    /// <summary>
    /// Generates a signed JWT Access Token for the specified user.
    /// </summary>
    string GenerateAccessToken(TokenGenerationRequest request);

    /// <summary>
    /// Generates a cryptographically secure refresh token, its SHA-256 hash, and its expiration timestamp.
    /// </summary>
    GeneratedRefreshToken GenerateRefreshToken(DateTime issuedAt);

    /// <summary>
    /// Hashes a raw Refresh Token string using SHA-256 (HEX output).
    /// </summary>
    string HashRefreshToken(string rawRefreshToken);
}
