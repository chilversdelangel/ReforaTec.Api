namespace ReforaTec.Api.Features.Users.GetMyTrees;

internal sealed record Response(
    int Id,
    int SpeciesId,
    string CommonName,
    string ScientificName,
    string ValueName,
    string? CurrentCampaignName,
    string? CurrentCampaignFolio,
    string HealthState,
    DateOnly? PlantingDate,
    decimal? CurrentHeightCentimeters,
    decimal? CurrentDiameterCentimeters,
    string? SpeciesImageUrl,
    string? LatestPhotoUrl);
