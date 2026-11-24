namespace ReforaTec.Api.Features.Species.CreateSpecies;

internal sealed record Request(
    string ScientificName,
    string CommonName,
    string Description,
    string ImageUrl);
