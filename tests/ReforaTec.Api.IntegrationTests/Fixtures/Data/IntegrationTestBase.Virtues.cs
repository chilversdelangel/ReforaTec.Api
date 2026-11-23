using Microsoft.Extensions.DependencyInjection;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;

// ReSharper disable once CheckNamespace
namespace ReforaTec.Api.IntegrationTests.Fixtures;

public abstract partial class IntegrationTestBase
{
    private static int _virtueSequence = 100;

    protected async Task<Virtue> CreateVirtueAsync(string? name = null)
    {
        using var scope = Factory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var sequence = Interlocked.Increment(ref _virtueSequence);

        var virtue = new Virtue
        {
            Name = name ?? $"Virtue {sequence}"
        };

        virtue.Normalize();

        appDbContext.Virtues.Add(virtue);
        await appDbContext.SaveChangesAsync();

        return virtue;
    }
}
