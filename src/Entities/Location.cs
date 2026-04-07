using Microsoft.EntityFrameworkCore;

namespace ReforaTec.Api.Entities;

[Owned]
public class Location
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string Street { get; set; }
    public string Neighborhood { get; set; }
    public string StreetNumber { get; set; }
}