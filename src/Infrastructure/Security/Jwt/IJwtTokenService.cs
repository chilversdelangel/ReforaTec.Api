using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Infrastructure.Security.Jwt;

public record RefreshTokenResult(string RawToken, string HashedToken);

public record TokenGenerationRequest(int UserId, UserRole Role, string Audience);

public interface IJwtTokenService
{
    /// <summary>
    /// Generates a signed JWT Access Token for the specified user.
    /// </summary>
    string GenerateAccessToken(TokenGenerationRequest request);

    /// <summary>
    /// Generates a cryptographically secure raw Refresh Token alongside its SHA-256 hash.
    /// </summary>
    RefreshTokenResult GenerateRefreshToken();
}
