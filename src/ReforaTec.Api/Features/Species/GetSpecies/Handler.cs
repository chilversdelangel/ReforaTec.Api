using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;

namespace ReforaTec.Api.Features.Species.GetSpecies;

internal static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var species = await context.Species
            .AsNoTracking()
            .OrderBy(species => species.CommonName)
            .Select(species => new SpeciesDto(
                species.Id,
                species.CommonName,
                species.ScientificName,
                species.Description,
                species.ImageUrl))
            .ToListAsync(cancellationToken);

        return new Response(species);
    }
}
