using System.Security.Claims;
using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Endpoints;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.OpenApi;

namespace ReforaTec.Api.Features.Users.GetMyTrees;

internal sealed class GetMyTrees : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/me/trees", HandleRequest)
            .WithName("GetMyAssignedTrees")
            .WithTags(OpenApiTags.Users)
            .Produces<List<Response>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get assigned trees for current user")
            .WithDescription("Retrieves the list of trees currently assigned to the authenticated user for care and monitoring.")
            .RequireAuthorization();
    }

    private static async Task<IResult> HandleRequest(
        ClaimsPrincipal user,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var result = await Handler.Handle(user, context, cancellationToken);

        return result.Match(
            Results.Ok,
            errors => errors.ToProblem());
    }
}
