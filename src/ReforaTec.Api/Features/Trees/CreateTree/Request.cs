namespace ReforaTec.Api.Features.Trees.CreateTree;

internal sealed record Request(
    string Folio,
    int SpeciesId,
    int VirtueId,
    DateOnly? PlantingDate,
    TreeLocationRequest? Location);

internal sealed record TreeLocationRequest(
    double? Latitude,
    double? Longitude);
