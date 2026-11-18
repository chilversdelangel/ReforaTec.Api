using Microsoft.Extensions.DependencyInjection;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Infrastructure.Security.Jwt;

// ReSharper disable once CheckNamespace
namespace ReforaTec.Api.IntegrationTests.Fixtures;

public abstract partial class IntegrationTestBase
{
    protected async Task<RefreshToken> CreateRefreshTokenAsync(
        int userId,
        string? hashedToken = null,
        string audience = Audience.StudentMobileApp,
        DateTime? expiresAt = null)
    {
        using var scope = Factory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var refreshToken = new RefreshToken
        {
            UserId = userId,
            HashedToken = hashedToken ?? Guid.NewGuid().ToString("N").PadRight(64, '0'),
            Audience = audience,
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(21)
        };

        appDbContext.RefreshTokens.Add(refreshToken);
        await appDbContext.SaveChangesAsync();

        return refreshToken;
    }
}
