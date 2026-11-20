using Microsoft.Extensions.DependencyInjection;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;

// ReSharper disable once CheckNamespace
namespace ReforaTec.Api.IntegrationTests.Fixtures;

public abstract partial class IntegrationTestBase
{
    private static int _careTypeSequence = 100;

    protected async Task<CareType> CreateCareTypeAsync(
        string? name = null,
        string? iconUrl = null,
        bool isActive = true)
    {
        using var scope = Factory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var sequence = Interlocked.Increment(ref _careTypeSequence);

        var careType = new CareType
        {
            Name = name ?? $"CustomCareType-{sequence}",
            IconUrl = iconUrl ?? $"https://icons.reforatec.org/care-types/custom-{sequence}.png",
            IsActive = isActive
        };

        careType.Normalize();

        appDbContext.CareTypes.Add(careType);
        await appDbContext.SaveChangesAsync();

        return careType;
    }
}
