using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Infrastructure.Security.Jwt;

namespace ReforaTec.Api.Features.Auth.RefreshSession;

public static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        Request request,
        AppDbContext context,
        IJwtTokenService jwtTokenService,
        CancellationToken cancellationToken = default)
    {
        var inputToken = request.RefreshToken;
        var hashedInputToken = jwtTokenService.HashRefreshToken(inputToken);

        var existingToken = await context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.HashedToken == hashedInputToken, cancellationToken);

        if (existingToken?.User is null || existingToken.User.IsDeleted || existingToken.Audience != request.Audience)
            return Error.Unauthorized(ErrorCodes.InvalidToken, "Invalid refresh token.");

        if (existingToken.ExpiresAt < DateTime.UtcNow)
        {
            context.RefreshTokens.Remove(existingToken);
            await context.SaveChangesAsync(cancellationToken);
            return Error.Unauthorized(ErrorCodes.TokenExpired, "Refresh token has expired. Please log in again.");
        }

        context.RefreshTokens.Remove(existingToken);

        var tokenRequest = new TokenGenerationRequest(
            existingToken.UserId,
            existingToken.User.CurrentRole,
            request.Audience);
        var newAccessToken = jwtTokenService.GenerateAccessToken(tokenRequest);

        var newRawRefreshToken = jwtTokenService.GenerateRawRefreshToken();
        var newHashedRefreshToken = jwtTokenService.HashRefreshToken(newRawRefreshToken);

        context.RefreshTokens.Add(new RefreshToken
        {
            UserId = existingToken.UserId,
            HashedToken = newHashedRefreshToken,
            Audience = request.Audience,
            ExpiresAt = DateTime.UtcNow.AddDays(21)
        });

        await context.SaveChangesAsync(cancellationToken);

        return new Response(newAccessToken, newRawRefreshToken);
    }
}
