namespace ReforaTec.Api.Entities.Common;

public record Location
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public required string Street { get; set; }
    public required string Neighborhood { get; set; }
    public required string StreetNumber { get; set; }
}