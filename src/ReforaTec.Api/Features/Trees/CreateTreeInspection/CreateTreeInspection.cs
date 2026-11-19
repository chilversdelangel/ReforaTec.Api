using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Endpoints;
using ReforaTec.Api.Infrastructure.Filters;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.OpenApi;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.Trees.CreateTreeInspection;

internal sealed class CreateTreeInspection : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/trees/{treeId:int}/inspections", HandleRequest)
            .WithName("CreateTreeInspection")
            .WithTags(OpenApiTags.Trees)
            .AddEndpointFilter<ValidationFilter<Request>>()
            .Produces<Response>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Record tree inspection")
            .WithDescription("Creates a tree health inspection.")
            .RequireAuthorization();
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

        var result = await Handler.Handle(treeId, identityResult.Value, request, context, cancellationToken);

        return result.Match(
            response => Results.Created($"/v1/trees/{treeId}/inspections/{response.Id}", response),
            errors => errors.ToProblem());
    }
}
