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
        IJwtService jwtService,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var hashedInputToken = jwtService.HashRefreshToken(request.RefreshToken);

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

        var newAccessToken = jwtService.GenerateAccessToken(tokenRequest);
        var refreshToken = jwtService.GenerateRefreshToken(now);

        context.RefreshTokens.Add(new RefreshToken
        {
            UserId = existingToken.UserId,
            HashedToken = refreshToken.HashedToken,
            Audience = request.Audience,
            ExpiresAt = refreshToken.ExpiresAt
        });

        await context.SaveChangesAsync(cancellationToken);

        return new Response(newAccessToken, refreshToken.RawToken);
    }
}
