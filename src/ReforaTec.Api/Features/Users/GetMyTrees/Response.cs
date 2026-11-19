namespace ReforaTec.Api.Features.Users.GetMyTrees;

internal sealed record Response(IReadOnlyList<AssignedTreeDto> Trees);

internal sealed record AssignedTreeDto(
    int Id,
    string Folio,
    
    int SpeciesId,
    string CommonName,
    string ScientificName,
    string? SpeciesImageUrl,
    
    string VirtueName,
    
    string HealthState,
    DateOnly? PlantingDate,
    
    decimal? CurrentHeightCentimeters,
    
    string? LatestPhotoUrl);
