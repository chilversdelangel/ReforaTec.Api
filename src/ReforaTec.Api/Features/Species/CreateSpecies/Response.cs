namespace ReforaTec.Api.Features.Species.CreateSpecies;

internal sealed record Response(
    int Id,
    string ScientificName,
    string CommonName,
    string Description,
    string ImageUrl);
