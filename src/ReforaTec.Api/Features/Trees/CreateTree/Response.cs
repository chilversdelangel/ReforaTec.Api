namespace ReforaTec.Api.Features.Trees.CreateTree;

internal sealed record Response(
    int Id,
    string Folio,
    int SpeciesId,
    int VirtueId,
    DateOnly? PlantingDate,
    TreeLocationDto? Location);

internal sealed record TreeLocationDto(
    double? Latitude,
    double? Longitude);
