using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Security.Jwt;

namespace ReforaTec.Api.Features.Auth.RevokeSession;

internal static class Handler
{
    public static async Task<ErrorOr<Success>> Handle(
        Request request,
        AppDbContext context,
        IJwtService jwtService,
        CancellationToken cancellationToken = default)
    {
        var inputToken = request.RefreshToken;
        var hashedToken = jwtService.HashRefreshToken(inputToken);

        var existingToken = await context.RefreshTokens
            .FirstOrDefaultAsync(refreshToken => refreshToken.HashedToken == hashedToken, cancellationToken);

        if (existingToken is null)
        {
            return Result.Success;
        }

        context.RefreshTokens.Remove(existingToken);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
