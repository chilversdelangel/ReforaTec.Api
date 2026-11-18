using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Infrastructure.Security.Jwt;

namespace ReforaTec.Api.Features.Auth.RefreshSession;

internal static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        Request request,
        AppDbContext context,
        IJwtTokenService jwtTokenService,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var hashedInputToken = jwtTokenService.HashRefreshToken(request.RefreshToken);

        var existingToken = await context.RefreshTokens
            .Include(refreshToken => refreshToken.User)
            .FirstOrDefaultAsync(refreshToken => refreshToken.HashedToken == hashedInputToken, cancellationToken);

        if (existingToken?.User is null || existingToken.Audience != request.Audience)
        {
            return Errors.InvalidToken;
        }

        if (existingToken.ExpiresAt < now)
        {
            context.RefreshTokens.Remove(existingToken);
            await context.SaveChangesAsync(cancellationToken);
            return Errors.TokenExpired;
        }

        context.RefreshTokens.Remove(existingToken);

        var tokenRequest = new TokenGenerationRequest(
            existingToken.UserId,
            existingToken.User.TenantId,
            existingToken.User.Role,
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
