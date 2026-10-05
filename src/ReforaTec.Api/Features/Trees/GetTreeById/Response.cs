namespace ReforaTec.Api.Features.Trees.GetTreeById;

internal sealed record Response(
    int Id,
    string? CurrentCampaignFolio,
    string HealthState,
    DateOnly? PlantingDate,
    string? Observations,
    string Value,
    string Species,
    string SpeciesScientificName,
    string? SpeciesImageUrl,
    decimal? CurrentHeightCentimeters,
    decimal? CurrentDiameterCentimeters,
    string? LatestPhotoUrl,
    LocationDto? Location,
    List<AssignedStudentDto> CurrentAssignedStudents);

internal sealed record LocationDto(
    double? Latitude,
    double? Longitude,
    string Street,
    string Neighborhood,
    string StreetNumber);

internal sealed record AssignedStudentDto(
    int Id,
    string? ControlNumber,
    string FirstName,
    string? MiddleName,
    string LastName,
    string? SecondLastName);
