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

    public string GenerateRawRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    public string HashRefreshToken(string rawRefreshToken)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(rawRefreshToken);
        var hashBytes = SHA256.HashData(tokenBytes);

        return Convert.ToHexString(hashBytes);
    }

    private static SigningCredentials CreateSigningCredentials(string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var securityKey = new SymmetricSecurityKey(keyBytes);

        return new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    }
}