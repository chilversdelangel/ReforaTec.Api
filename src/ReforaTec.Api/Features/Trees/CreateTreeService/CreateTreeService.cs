using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Endpoints;
using ReforaTec.Api.Infrastructure.Filters;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.OpenApi;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.Trees.CreateTreeService;

internal sealed class CreateTreeService : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/trees/{treeId:int}/services", HandleRequest)
            .WithName("CreateTreeService")
            .WithTags(OpenApiTags.Trees)
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter<Request>>()
            .Produces<Response>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .WithSummary("Log a tree maintenance service")
            .WithDescription("Registers a service log (watering, pruning, etc.) for a tree assigned to the authenticated student.");
    }

    private static async Task<IResult> HandleRequest(
        int treeId,
        Request request,
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
            treeId,
            identityResult.Value,
            request,
            context,
            cancellationToken);

        return result.Match(
            response => Results.CreatedAtRoute("GetTreeServices", new { treeId }, response),
            errors => errors.ToProblem());
    }
}
