using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ReforaTec.Api.Infrastructure.Security.Jwt;

public class JwtTokenService(IOptions<JwtOptions> jwtOptions) : IJwtTokenService
{
    private readonly JwtOptions _options = jwtOptions.Value;
    private readonly SigningCredentials _signingCredentials = CreateSigningCredentials(jwtOptions.Value.SecretKey);

    public string GenerateAccessToken(TokenGenerationRequest request)
    {
        var expiresAt = DateTime.UtcNow.AddHours(_options.AccessTokenExpirationHours);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, request.UserId.ToString()),
            new Claim(ClaimTypes.Role, request.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: request.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: _signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshTokenResult GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        var rawToken = Convert.ToBase64String(randomBytes);
        var hashedToken = HashToken(rawToken);

        return new RefreshTokenResult(rawToken, hashedToken);
    }

    private static SigningCredentials CreateSigningCredentials(string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var securityKey = new SymmetricSecurityKey(keyBytes);

        return new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    }

    private static string HashToken(string rawToken)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(rawToken);
        var hashBytes = SHA256.HashData(tokenBytes);

        return Convert.ToHexString(hashBytes);
    }
}