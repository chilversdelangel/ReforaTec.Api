using ErrorOr;
using Mapster;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;

namespace ReforaTec.Api.Features.Species.GetSpeciesById;

public static class GetSpeciesById
{
    public record Response(
        int Id,
        string ScientificName,
        List<string> CommonNames,
        string Description,
        string? ImageUrl,
        DateTime CreatedAt,
        DateTime ModifiedAt
    );

    private static class ErrorCodes
    {
        public const string NotFound = "Species.NotFound";
    }

    public static async Task<ErrorOr<Response>> Handle(int id, AppDbContext context)
    {
        var species = await context.Species
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (species == null)
            return Error.NotFound(
                code: ErrorCodes.NotFound,
                description: $"Species with ID {id} not found"
            );

        return species.Adapt<Response>();
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/species/{id:int}", async (int id, AppDbContext context) =>
            {
                var result = await Handle(id, context);

                return result.Match(
                    Results.Ok,
                    errors => Results.NotFound(
                        new
                        {
                            detail = errors[0].Description,
                            code = errors[0].Code
                        })
                );
            })
            .WithName("GetSpeciesById")
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }
}