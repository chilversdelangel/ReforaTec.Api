using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Endpoints;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.OpenApi;

namespace ReforaTec.Api.Features.Trees.GetTreeById;

internal sealed class GetTreeById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/trees/{id:int}", HandleRequest)
            .WithName("GetTreeById")
            .WithTags(OpenApiTags.Trees)
            .Produces<Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get tree technical profile")
            .WithDescription("Retrieves the complete technical sheet, botanical details, current measurements, and active assigned students for a specific tree.")
            .RequireAuthorization();
    }

    private static async Task<IResult> HandleRequest(
        int id,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var result = await Handler.Handle(id, context, cancellationToken);

        return result.Match(
            Results.Ok,
            errors => errors.ToProblem());
    }
}