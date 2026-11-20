using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Endpoints;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.OpenApi;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.Trees.GetTrees;

internal sealed class GetTrees : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/trees", HandleRequest)
            .WithName("GetTrees")
            .WithTags(OpenApiTags.Trees)
            .Produces<Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .WithSummary("List tenant trees")
            .WithDescription("Retrieves a summary list of trees within the authenticated user's tenant, ordered by folio.")
            .RequireAuthorization();
    }

    private static async Task<IResult> HandleRequest(
        ICurrentUserService currentUser,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var identityResult = currentUser.GetIdentity();
        if (identityResult.IsError)
        {
            return identityResult.Errors.ToProblem();
        }

        var result = await Handler.Handle(
            identityResult.Value,
            context,
            cancellationToken);

        return result.Match(
            Results.Ok,
            errors => errors.ToProblem());
    }
}
