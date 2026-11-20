using Microsoft.Extensions.DependencyInjection;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;

// ReSharper disable once CheckNamespace
namespace ReforaTec.Api.IntegrationTests.Fixtures;

public abstract partial class IntegrationTestBase
{
    private static int _speciesSequence = 100;

    protected async Task<Species> CreateSpeciesAsync(
        string? commonName = null,
        string? scientificName = null,
        string? description = null,
        string? imageUrl = null)
    {
        using var scope = Factory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var sequence = Interlocked.Increment(ref _speciesSequence);

        var species = new Species
        {
            CommonName = commonName ?? $"Common Species {sequence}",
            ScientificName = scientificName ?? $"Scientificus speciei {sequence}",
            Description = description ?? $"Botanical description for test species {sequence}.",
            ImageUrl = imageUrl ?? $"https://storage.reforatec.org/species/species-{sequence}.jpg"
        };

        species.Normalize();

        appDbContext.Species.Add(species);
        await appDbContext.SaveChangesAsync();

        return species;
    }
}
