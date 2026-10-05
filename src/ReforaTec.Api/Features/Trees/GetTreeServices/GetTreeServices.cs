using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Endpoints;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.OpenApi;

namespace ReforaTec.Api.Features.Trees.GetTreeServices;

internal sealed class GetTreeServices : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/trees/{treeId:int}/services", HandleRequest)
            .WithName("GetTreeServices")
            .WithTags(OpenApiTags.Trees)
            .Produces<List<Response>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get tree service history")
            .WithDescription("Retrieves the list of all maintenance and care services logged for a specific tree, ordered by capture date descending.")
            .RequireAuthorization();
    }

    private static async Task<IResult> HandleRequest(
        int treeId,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var result = await Handler.Handle(treeId, context, cancellationToken);

        return result.Match(
            Results.Ok,
            errors => errors.ToProblem());
    }
}
