using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Database;

namespace ReforaTec.Api.Features.Species.CreateSpecies;

internal static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        Request request,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var normalizedScientificName = request.ScientificName.ToNormalized();

        if (await ScientificNameExistsAsync(context, normalizedScientificName, cancellationToken))
        {
            return Errors.DuplicateScientificName(request.ScientificName.ToSanitized());
        }

        var species = new Entities.Species
        {
            ScientificName = request.ScientificName,
            CommonName = request.CommonName,
            Description = request.Description.ToSanitized(),
            ImageUrl = request.ImageUrl.ToSanitized()
        };

        context.Species.Add(species);
        await context.SaveChangesAsync(cancellationToken);

        return new Response(
            species.Id,
            species.ScientificName,
            species.CommonName,
            species.Description,
            species.ImageUrl);
    }

    private static Task<bool> ScientificNameExistsAsync(
        AppDbContext context,
        string normalizedScientificName,
        CancellationToken cancellationToken)
    {
        return context.Species
            .AsNoTracking()
            .Where(species => species.NormalizedScientificName == normalizedScientificName)
            .AnyAsync(cancellationToken);
    }
}
