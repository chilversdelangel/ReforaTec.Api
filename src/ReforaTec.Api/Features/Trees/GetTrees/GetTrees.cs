using ReforaTec.Api.Infrastructure.Endpoints;

namespace ReforaTec.Api.Features.Trees.GetTrees;

internal sealed class GetTrees : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/trees", Handle);
    }

    public static Task<List<Response>> Handle()
    {
        var treeList = new List<Response>();
        return Task.FromResult(treeList);
    }

    public record LocationResponse(
        double? Latitude,
        double? Longitude,
        string Street,
        string Neighborhood,
        string StreetNumber
    );

    public record Response(
        int Id,
        DateTime CreatedAt,
        DateTime ModifiedAt,
        DateOnly PlantingDate,
        int ValueId,
        int SpeciesId,
        decimal? Height,
        decimal? Diameter,
        LocationResponse Location,
        string? Notes
    );
}