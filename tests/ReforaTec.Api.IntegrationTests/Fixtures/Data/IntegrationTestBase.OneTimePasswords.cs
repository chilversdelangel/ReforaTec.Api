using Microsoft.Extensions.DependencyInjection;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;

// ReSharper disable once CheckNamespace
namespace ReforaTec.Api.IntegrationTests.Fixtures;

public abstract partial class IntegrationTestBase
{
    protected async Task<OneTimePassword> CreateOneTimePasswordAsync(
        int userId,
        string code = "123456",
        int failedAttempts = 0,
        DateTime? expiresAt = null)
    {
        using var scope = Factory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var oneTimePassword = new OneTimePassword
        {
            UserId = userId,
            Code = code,
            FailedAttempts = failedAttempts,
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddMinutes(5)
        };

        appDbContext.OneTimePasswords.Add(oneTimePassword);
        await appDbContext.SaveChangesAsync();

        return oneTimePassword;
    }
}
