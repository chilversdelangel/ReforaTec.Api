using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Security.Jwt;

namespace ReforaTec.Api.Features.Auth.RevokeSession;

public static class Handler
{
    public static async Task<ErrorOr<Success>> Handle(
        Request request,
        AppDbContext context,
        IJwtTokenService jwtTokenService,
        CancellationToken cancellationToken = default)
    {
        var inputToken = request.RefreshToken;
        var hashedToken = jwtTokenService.HashRefreshToken(inputToken);

        var existingToken = await context.RefreshTokens
            .FirstOrDefaultAsync(r => r.HashedToken == hashedToken, cancellationToken);

        if (existingToken is null) return Result.Success;
        
        context.RefreshTokens.Remove(existingToken);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
