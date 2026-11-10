namespace ReforaTec.Api.Features.Users.GetMyTrees;

internal sealed record Response(IReadOnlyList<AssignedTreeDto> Trees);

internal sealed record AssignedTreeDto(
    int Id,
    int SpeciesId,
    string CommonName,
    string ScientificName,
    string VirtueName,
    string? CurrentCampaignName,
    string? CurrentCampaignFolio,
    string HealthState,
    DateOnly? PlantingDate,
    decimal? CurrentHeightCentimeters,
    decimal? CurrentDiameterCentimeters,
    string? SpeciesImageUrl,
    string? LatestPhotoUrl);
