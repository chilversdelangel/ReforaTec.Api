namespace ReforaTec.Api.Features.Trees.GetTrees;

public static class GetTrees
{
    public record LocationResponse(
        double[] Coordinates,
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

    public static Task<List<Response>> Handle()
    {
        var treeList = new List<Response> { };
        return Task.FromResult(treeList);
    }

    public static void MapEndpoint(WebApplication app)
    {
        app.MapGet("/trees", Handle);
    }
}