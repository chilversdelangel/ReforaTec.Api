using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Endpoints;
using ReforaTec.Api.Infrastructure.Filters;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.OpenApi;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.Trees.CreateTree;

internal sealed class CreateTree : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/trees", HandleRequest)
            .WithName("CreateTree")
            .WithTags(OpenApiTags.Trees)
            .AddEndpointFilter<ValidationFilter<Request>>()
            .Produces<Response>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithSummary("Register a new tree")
            .WithDescription("Registers a new cataloged tree in the campus inventory with its botanical species, institutional virtue, and optional GPS coordinates.")
            .RequireAuthorization();
    }

    private static async Task<IResult> HandleRequest(
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

        var result = await Handler.Handle(identityResult.Value, request, context, cancellationToken);

        return result.Match(
            response => Results.CreatedAtRoute("GetTreeById", new { treeId = response.Id }, response),
            errors => errors.ToProblem());
    }
}
