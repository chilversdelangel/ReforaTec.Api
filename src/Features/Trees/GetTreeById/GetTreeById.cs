using Mapster;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;

namespace ReforaTec.Api.Features.Trees.GetTreeById;

public static class GetTreeById
{
    public record LocationDto(
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
        LocationDto Location,
        string? Notes
    );

    public static async Task<IResult> Handle(int id, AppDbContext context)
    {
        var tree = await context.Trees
            .AsNoTracking()
            .SingleOrDefaultAsync(t => t.Id == id);

        if (tree is null)
        {
            return Results.Problem(
                statusCode: 404,
                title: "Tree not found",
                detail: $"The tree with ID {id} does not exist."
            );
        }

        return Results.Ok(tree.Adapt<Response>());
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/trees/{id:int}", Handle)
            .WithName("GetTreeById");
    }
}