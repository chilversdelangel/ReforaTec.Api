namespace ReforaTec.Api.Features.Common.Dtos;

public record LocationDto(
    double? Latitude,
    double? Longitude,
    string Street,
    string Neighborhood,
    string StreetNumber
);