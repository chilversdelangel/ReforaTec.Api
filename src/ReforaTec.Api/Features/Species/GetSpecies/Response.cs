namespace ReforaTec.Api.Features.Species.GetSpecies;

internal sealed record Response(IReadOnlyList<SpeciesDto> Species);

internal sealed record SpeciesDto(
    int Id,
    string CommonName,
    string ScientificName,
    string Description,
    string? ImageUrl);
